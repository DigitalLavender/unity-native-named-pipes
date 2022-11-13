#pragma once

class pipe_client
{
protected:
    pipe_client() = default;

public:
    pipe_client(const pipe_client&) = delete;
    pipe_client& operator =(pipe_client const&) = delete;
    pipe_client(pipe_client&&) = delete;
    pipe_client& operator=(pipe_client&&) = delete;
    
    virtual ~pipe_client() = default;

    static pipe_client* create(enum class pipe_direction direction);
    static void remove(pipe_client* client);

    virtual bool is_connected() = 0;

    virtual int read    (unsigned char* buffer, int size) = 0;
    virtual int write   (unsigned char* buffer, int size) = 0;
    virtual int open    (const wchar_t* name) = 0;
    virtual int close   () = 0;
};
