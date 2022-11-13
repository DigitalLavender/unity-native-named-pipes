
#include "framework.hh"
#include "pipe_client.hh"
#include "enums/pipe_direction.hh"
#include "enums/client_error_types.hh"
#include "utils/casters.hh"
#include "utils/logger.hh"

#if !defined(_NDEBUG)
#include <iostream>
#endif

class pipe_client_windows final : public pipe_client
{
public:
    explicit pipe_client_windows(pipe_direction direction);

    pipe_client_windows(const pipe_client_windows&) = delete;
    pipe_client_windows& operator =(pipe_client_windows const&) = delete;
    pipe_client_windows(pipe_client_windows&&) = delete;
    pipe_client_windows& operator=(pipe_client_windows&&) = delete;

    ~pipe_client_windows() override = default;

private:
    HANDLE  handle_;
    bool    is_opened_;
    int     desired_access_;

    int     open_counter_ = 0;

public:
    bool    is_connected() override;

    int     read    (unsigned char* buffer, int size) override;
    int     write   (unsigned char* buffer, int size) override;
    int     open    (const wchar_t* name) override;
    int     close   () override;
};

pipe_client_windows::pipe_client_windows(pipe_direction direction)
{
    handle_ = INVALID_HANDLE_VALUE;
    is_opened_ = false;

    switch (direction) {
        case pipe_direction::in: desired_access_ = -2147483648; break;
        case pipe_direction::out: desired_access_ = 1073741824; break;
        case pipe_direction::in_out: desired_access_ = GENERIC_READ | GENERIC_WRITE; break;
    }
}

bool pipe_client_windows::is_connected()
{
    if (handle_ == nullptr) return false;
    if (handle_ == INVALID_HANDLE_VALUE) return false;

    return is_opened_;
}

int pipe_client_windows::read(unsigned char* buffer, int size)
{
    if (size < 1)
    {
        return to_underlying(client_error_types::too_small_size);
    }

    if (!is_connected())
    {
#if !defined(_NDEBUG)
        std::cout << "pipe_client_windows::read: not connected, but try to read. " << std::endl;
#endif
        return to_underlying(client_error_types::not_connected);
    }

    DWORD bytes_available = 0;
    if (::PeekNamedPipe(handle_, nullptr, 0, nullptr, &bytes_available, nullptr))
    {
        if (bytes_available > 0)
        {
            const auto bytes_to_read = static_cast<DWORD>(size);
            DWORD bytes_read = 0;

            if (::ReadFile(handle_, buffer, bytes_to_read, &bytes_read, nullptr))
            {
                return static_cast<int>(bytes_read);
            }
            else
            {
                close();
                return to_underlying(client_error_types::read_file_failed);
            }
        }
        else
        {
            return 0;
        }
    }
    else
    {
        close();
        return to_underlying(client_error_types::failed_to_peek);
    }
}

int pipe_client_windows::write(unsigned char* buffer, int size)
{
    if (size < 1) return to_underlying(client_error_types::too_small_size);
    if (!is_connected()) return to_underlying(client_error_types::not_connected);

    DWORD bytes_written = 0;
    if (::WriteFile(handle_, buffer, static_cast<DWORD>(size), &bytes_written, nullptr) == 1)
    {
        return static_cast<int>(bytes_written);
    }
    else
    {
        close();
        return to_underlying(client_error_types::write_file_failed);
    }
}

int pipe_client_windows::open(const wchar_t* name)
{
    ++open_counter_;

#if !defined(_NDEBUG)
    std::wcout << "pipe_client_windows::open: " << name << "(" << open_counter_ << ")" << std::endl;
#endif

    this->handle_ = ::CreateFile(name, desired_access_, 0, nullptr,OPEN_EXISTING, 0, nullptr);

    if (handle_ != INVALID_HANDLE_VALUE)
    {
        is_opened_ = true;

#if !defined(_NDEBUG)
        std::wcout << "pipe_client_windows::open success. (" << open_counter_ << ")" << std::endl;
#endif
        
        return to_underlying(client_error_types::ok);
    }

    const auto err = GetLastError();
    const int out_error = static_cast<int>(err);
    
    if (err == ERROR_PIPE_BUSY)
    {
        if (!WaitNamedPipe(name, 1000))
        {
            return out_error;
        }
        else
        {
            return open(name);
        }
    }

    return out_error;
}

int pipe_client_windows::close()
{
#if _DEBUG
    if (handle_ == nullptr) return to_underlying(client_error_types::null_handle);
    if (handle_ == INVALID_HANDLE_VALUE) return to_underlying(client_error_types::invalid_handle);
    if (!is_opened_) return to_underlying(client_error_types::not_connected);
#endif

    ::CloseHandle(handle_);

    this->handle_ = INVALID_HANDLE_VALUE;
    this->is_opened_ = false;

    return 0;
}

#if _WIN64
pipe_client* pipe_client::create(pipe_direction direction)
{
    return new pipe_client_windows(direction);
}
#endif
