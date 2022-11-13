#include "framework.hh"
#include "pipe_client.hh"
#include "enums/pipe_direction.hh"
#include "enums/client_error_types.hh"
#include "utils/casters.hh"
#include "utils/logger.hh"

class pipe_client_unix final : public pipe_client
{
public:
    explicit pipe_client_unix(pipe_direction direction);

    pipe_client_unix(const pipe_client_unix&) = delete;
    pipe_client_unix& operator=(pipe_client_unix const&) = delete;
    pipe_client_unix(pipe_client_unix&&) = delete;
    pipe_client_unix& operator=(pipe_client_unix&&) = delete;

    ~pipe_client_unix() override = default;

private:
    HANDLE  handle_;
    bool    is_opened_;
    int     desired_access_;

public:
    bool    is_connected() override;

    int     read    (unsigned char* buffer, int size) override;
    int     write   (unsigned char* buffer, int size) override;
    int     open    (const wchar_t* name) override;
    int     close   () override;
};

#if defined(__unix__)
pipe_client* pipe_client::create(pipe_direction direction)
{
    return new pipe_client_unix(direction);
}
#endif