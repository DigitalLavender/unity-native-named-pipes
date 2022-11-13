#include "pipe_client.hh"

void pipe_client::remove(pipe_client* client)
{
    if (client != nullptr)
    {
        if (client->is_connected())
        {
            client->close();
        }

        delete client;
        client = nullptr;
    }
}
