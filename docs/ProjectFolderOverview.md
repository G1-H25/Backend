# Project Folder Overview

---

## /Composition

**What it does:**  
- Sets up the application at startup  
- Connects different parts like services, middleware, and tools (e.g., API documentation)

---

## /Configuration

**What it does:**  
- Manages app settings and environment details  
- Handles database connection info and helps the app find its settings

---

## /Middleware

**What it does:**  
- Contains small building blocks that run between the app and incoming requests  
- Can log activity, handle errors, check user access, or measure performance

---

## /Repository

**What it does:**  
- Reads from and writes to the database or other storage  
- Acts as a middleman between the app and data storage

---

## /Models

**What it does:**  
- Defines the shape of data the app works with  
- Includes how information is organized when sent or received  
- Covers data used internally or from outside sources

---

## /Controllers

**What it does:**  
- Handles incoming requests from users or other apps  
- Connects user actions to the app’s logic and data  
- Sends back responses

---
