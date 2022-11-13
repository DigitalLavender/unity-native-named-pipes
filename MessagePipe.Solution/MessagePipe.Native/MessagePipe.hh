#pragma once

#if MESSAGEPIPENATIVE_EXPORTS
    #define MESSAGEPIPE_API __declspec(dllexport)
#else
    #define MESSAGEPIPE_API __declspec(dllimport)
#endif

enum class pipe_direction;

#pragma region client

class pipe_client;
using h_client = pipe_client*;

extern "C" {
    MESSAGEPIPE_API h_client        pipe_client_create(pipe_direction direction);
    MESSAGEPIPE_API void            pipe_client_remove(h_client client);

    MESSAGEPIPE_API bool            pipe_client_is_connected(h_client client);

    MESSAGEPIPE_API int             pipe_client_read(h_client client, unsigned char* buffer, int size);
    MESSAGEPIPE_API int             pipe_client_write(h_client client, unsigned char* buffer, int size);
    MESSAGEPIPE_API int             pipe_client_open(h_client client, const wchar_t* name);
    MESSAGEPIPE_API int             pipe_client_close(h_client client);
}
#pragma endregion client

#pragma region server

class pipe_server;
using h_server = pipe_server*;

extern "C" {
    MESSAGEPIPE_API h_server        pipe_server_create(pipe_direction direction);
    MESSAGEPIPE_API void            pipe_server_remove(h_server server);

    MESSAGEPIPE_API bool            pipe_server_is_connected(h_server server);

    MESSAGEPIPE_API int             pipe_server_read(h_server server, unsigned char* buffer, int size);
    MESSAGEPIPE_API int             pipe_server_write(h_server server, unsigned char* buffer, int size);
    MESSAGEPIPE_API int             pipe_server_open(h_server server, const wchar_t* name);
    MESSAGEPIPE_API int             pipe_server_close(h_server server);
}
#pragma endregion server
