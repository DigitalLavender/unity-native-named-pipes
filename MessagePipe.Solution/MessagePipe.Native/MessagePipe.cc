#include "MessagePipe.hh"

#pragma region client

#include "pipes/pipe_client.hh"

h_client pipe_client_create(pipe_direction direction)
{
    return pipe_client::create(direction);
}

void pipe_client_remove(h_client client)
{
    pipe_client::remove(client);
}

bool pipe_client_is_connected(h_client client)
{
    return client->is_connected();
}

int pipe_client_read(h_client client, unsigned char* buffer, int size)
{
    return client->read(buffer, size);
}

int pipe_client_write(h_client client, unsigned char* buffer, int size)
{
    return client->write(buffer, size);
}

int pipe_client_open(h_client client, const wchar_t* name)
{
    return client->open(name);
}

int pipe_client_close(h_client client)
{
    return client->close();
}

#pragma endregion client

#pragma region server
#include "pipes/pipe_server.hh"

h_server pipe_server_create(pipe_direction direction)
{
    return nullptr;
}

void pipe_server_remove(h_server server)
{
}

bool pipe_server_is_connected(h_server server)
{
    return false;
}

int pipe_server_read(h_server server, unsigned char* buffer, int size)
{
    return 0;
}

int pipe_server_write(h_server server, unsigned char* buffer, int size)
{
    return 0;
}

int pipe_server_open(h_server server, const wchar_t* name)
{
    return 0;
}

int pipe_server_close(h_server server)
{
    return 0;
}

#pragma endregion server