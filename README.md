# ToroTrack

ToroTrack is a modern web application built with **Blazor (C\#)** and **PostgreSQL**, designed to be fully containerized using **Docker**.

## Project Architecture

This project uses a **Containerized Continuous Deployment** workflow:

1.  **Local Dev:** Runs entirely in Docker (App + DB) on your machine.
2.  **Source Control:** Code is pushed to GitHub.
3.  **Deployment:**
      * Images are built and pushed to **Docker Hub**.
      * An **Azure Webhook** triggers the App Service to pull the new image.
      * The application automatically applies **Database Migrations** upon startup in both environments.

## Tech Stack

  * **Framework:** .NET 8 (Blazor Interactive Server)
  * **Database:** PostgreSQL 16
  * **Containerization:** Docker & Docker Compose
  * **Cloud Hosting:** Azure App Service (Linux Containers)
  * **ORM:** Entity Framework Core

## How to Run Locally

You do not need to install PostgreSQL locally. Docker handles everything.

### Prerequisites

  * [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Must be running)
  * [Git](https://git-scm.com/)

### 1\. Clone the Repository

```bash
git clone https://github.com/CodeByMathabo/ToroTrack.git
cd ToroInformatics
```

### 2\. Build and Start

Run these commands from the root `ToroInformatics` folder:

```bash
# Build the C# App image
docker compose build torotrack

# Start the App and Database in the background
docker compose up -d
```

  * **Web App:** Accessible at `http://localhost:8080`
  * **Database:** Accessible at `localhost:5432`

### 3\. Access the Database (Optional)

You can connect to the local database using tools like **DBeaver**:

  * **Host:** `localhost`
  * **Port:** `5432`
  * **Database:** `torodb`
  * **Username:** `user`
  * **Password:** `password`
    *(Credentials are defined in `docker-compose.yml`)*

### 4\. Stop the App

```bash
docker compose down
```

## Deployment Workflow

To deploy updates to the Azure Production environment:

1.  **Commit your changes:**

    ```bash
    git add .
    git commit -m "Your feature description"
    git push origin main
    ```

2.  **Push to Docker Hub:**

    ```bash
    docker compose build torotrack
    docker compose push torotrack
    ```

3.  **Done\!**

      * Docker Hub triggers the Azure Webhook.
      * Azure restarts the app.
      * The app applies any new database migrations automatically.
      * The site is live in \~60 seconds.

## Database Migrations

The application is configured to apply pending migrations automatically when the container starts.

To create a *new* migration (after changing your C\# models):

1.  Open your terminal in the `ToroTrack` project folder (not the root).
2.  Run: `dotnet ef migrations add YourMigrationName`
3.  Rebuild and restart the Docker container to apply it.
