#include <iostream>
#include "packages/Catch2/catch.hpp"
#include "packages/Crow/crow_all.h"
// #include "external/asio/asio.hpp"

int main() {
    crow::SimpleApp app;

    CROW_ROUTE(app, "/")([]() {
        return "Hello, Crow!";
    });

    // curl -X POST http://localhost:18080/json      -H "Content-Type: application/json"      -d '{"name": "ERIKA"}'
    CROW_ROUTE(app, "/json").methods(crow::HTTPMethod::Post)([](const crow::request& req) {
        auto json = crow::json::load(req.body);
        if (!json)
            return crow::response(400, "Invalid JSON");

        std::string name = json["name"].s();
        return crow::response("Hello, " + name + "!");
    });

    app.port(18080).run();
}