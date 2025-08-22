#include <iostream>
#include "packages/Catch2/catch.hpp"
#include "packages/Crow/crow_all.h"
// #include "external/asio/asio.hpp"

int main() {
    crow::SimpleApp app;

    CROW_ROUTE(app, "/")([]() {
        return "Hello, Crow!";
    });

    app.port(18080).run();
}