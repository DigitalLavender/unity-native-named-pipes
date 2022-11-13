#pragma once

enum class client_error_types
{
    ok = 0,
    unknown = -1,

    not_connected = -1001,

    null_handle = -2001,
    invalid_handle = -2002,

    too_small_size = -3001,
    read_file_failed = -3002,
    failed_to_peek = -3003,
    write_file_failed = -3004,
};