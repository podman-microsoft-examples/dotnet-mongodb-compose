## Podman Compose on Windows 11 

#### Overview 

This project is an example of using Podman-Compose on your Windows machine as an alternative to Docker. In this example we'll spin up and populate a MongoDB instance as well as our own Dotnet 7 application, which will expose some basic CRUD operations. 

From a coding perspective this project was "borrowed" (👀) from the Learn.Microsoft website. If you're interested in building this example from scratch then please visit the microsoft guide which can be found >>(here)[https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-7.0&tabs=visual-studio]<<

> **NOTE** a few adjustments were made to the source code to allow for containerisation, please checkout the `BookService.cs` class to see how we (very basically) bootstrap our application using injected environmental variables 

The goals of this project is to not only containerise an instance of MongoDB and our application, but to be able to fully compose a fairly sterotypical development stack in an automated fashion using podman.  

#### Pre-Reqs

Firstly, versions of software being used when this example was built: 

- Windows 11 Home OS Build 22000.1817 
- WSL (Windows Subsystem for Linux, automatically installed by Podman)  
- Podman: 4.5.0
- Podman-Compose: 1.0.6
- DotNet: 7.0.203
- Python: 3.11.3
- PIP3: pip 23.1.2 
- MongoDB: 6.3.0
- MongoSH: 1.6.1
- Podman Desktop

##### Windows Terminal (Optional)

 It is also recommended to install the modern "Windows Terminal," which provides a superior user experience to the standard PowerShell and CMD prompts, as well as a WSL prompt, should you want it.

 You can install it by searching the Windows Store or by running the following winget command:

```bash 
$ winget install Microsoft.WindowsTerminal
``` 

> **NOTE** to install Podman Desktop: `$ winget install RedHat.Podman-Desktop`

##### Podman

Podman comes in 2 flavors. There is the Podman CLI which can be used to run, build and manage [OCI](https://opencontainers.org/) containers and container images. 

Podman desktop as you'd image provides the graphical tooling ontop of Podman to provide that seamless experience. Podman Desktop is optional, and wont be covered in this project.     

Podman can be installed in a number of ways, you can goto the [Podman GitHub Release Page](https://github.com/containers/podman/releases) and download the latest `podman-<RELEASE>-setup.exe`

Alternatively to install via the CLI using winget: 

`$ winget install Redhat.Podman`

##### Dotnet

The Dotnet CLI is used to buld and test the application locally. If you only want to test containerisation and buld the app via a multi-stage containerfile then the DotNet CLI is not required. 

```bash
$ winget install Microsoft.DotNet.SDK.7
```
##### Python3 & PIP3

`Podman-compose` has a hard dependency on Python3 and is installed via PIP thus we need to install these: 

Install Python : `$ winget install Python.Python.3.11`
Upgrade PIP: `$ python -m pip3 install --upgrade pip`

##### Podman Compose

With python install, we can now install Podman Compose. 

`$ pip3 install podman-compose `

##### Podman Desktop (optional)

Should you want some Visual desktop goodness ala Docker Desktop, then you can install Podman Desktop. To install Podman Desktop via executable please visit the [Podman Desktop](https://podman-desktop.io/downloads) download page. 

Alternatively to install via the CLI using winget:

`winget install RedHat.Podman-Desktop`

##### MongoDB and Shell

Finally and optionally if you want to test the codeabse against your local machine before containerisation, we need to install MongoDB Server and its corresponding client CLI: 

```bash
$ winget install -e --id MongoDB.Server 
```
> **NOTE** You will need to add the Mongo bin folder to your $PATH to make the MongoDB Server CLI accessible! For me this was located at: `C:/Program Files/MongoDB/Server/<VERSION>/bin`

Install the MongoDB Client shell: 

```bash
>> winget install -e --id MongoDB.Shell
```

🕺🕺 and now we're ready to go... 🕺🕺

### Deloying and Testing on your local machine

#### Setup MongoDB 

There are a few steps we need to go through to get MongoDB working locally with some dummy data for us to test against. 

1. Create a directory to store the MongoDB database e.g. `$ mkdir /MongoDB/Bookstore`
2. Now create & start a MongoDB database in this directory: `$ mongod --dbpath C:\MongoDB\Bookstore`
3. In a new terminal window connect to MongoDB by simply typing: `$ mongosh `
4. Change to the `BookStore` databse and create the collection `Books`: 

```bash 
test> use BookStore
switched to db BookStore

BookStore> db.createCollection('Books')
{ "ok" : 1 }
```
5. add some data! 

```bash
BookStore> db.Books.insertMany([{ "Name": "Design Patterns", "Price": 54.93, "Category": "Computers", "Author": "Ralph Johnson" }, { "Name": "Clean Code", "Price": 43.15, "Category": "Computers","Author": "Robert C. Martin" }])
{
    "acknowledged" : true,
    "insertedIds" : [
        ObjectId("61a6058e6c43f32854e51f51"),
        ObjectId("61a6058e6c43f32854e51f52")
     ]
 }
```

6. Finally, to test its all working: 

```bash
BookStore> db.Books.find().pretty()
{
     "_id" : ObjectId("61a6058e6c43f32854e51f51"),
     "Name" : "Design Patterns",
     "Price" : 54.93,
     "Category" : "Computers",
     "Author" : "Ralph Johnson"
 }
 {
     "_id" : ObjectId("61a6058e6c43f32854e51f52"),
     "Name" : "Clean Code",
     "Price" : 43.15,
     "Category" : "Computers",
     "Author" : "Robert C. Martin"
 }
```

#### Test the App

Assuming you've already cloned this repo onto your local machine, jump into the root directory. 

1. `appsettings.json` defines our MongoDB connection parameters. These are default and should remain unchanged, however adjust accordingly if you've configured MongoDB to start on a different port: 
   
   ```json 
   "BookStoreDatabase": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "BookStore",
    "BooksCollectionName": "Books"
  }
   ```

2. Start up the application using `dotnet run`

```bash
dotnet run
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5162
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\Users\<USER>\Documents\DotNet\BookStoreAPI
```
The Applications REST API consists of the following end points: 

- **GET**: /api/books
- **POST**: /api/books
- **GET**: /api/books/{id}
- **PUT**: /api/books/{id}
- **DELETE**: /api/books/{id}


##### Test via cURL 

We can use cURL to execute the REST API from the command line like so: 

```bash
curl http://localhost:5162/api/Books                                                                              

StatusCode        : 200
StatusDescription : OK
Content           : [{"Id":"644a9231cc7d83d21b04f6b7","Name":"Design Patterns","Price":54.93,"Category":"Computers","Author":"Ralph
                    Johnson"},{"Id":"644a9231cc7d83d21b04f6b8","Name":"Clean Code","Price":43.15,"Category":...
RawContent        : HTTP/1.1 200 OK
                    Transfer-Encoding: chunked
                    Content-Type: application/json; charset=utf-8
                    Date: Fri, 28 Apr 2023 18:39:07 GMT
                    Server: Kestrel

                    [{"Id":"644a9231cc7d83d21b04f6b7","Name":"Design Pat...
Forms             : {}
Headers           : {[Transfer-Encoding, chunked], [Content-Type, application/json; charset=utf-8], [Date, Fri, 28 Apr 2023
                    18:39:07 GMT], [Server, Kestrel]}
Images            : {}
InputFields       : {}
Links             : {}
ParsedHtml        : System.__ComObject
RawContentLength  : 241
```

##### Test via Browser 

##### Browser 

1. Open your browser of choice e.g. Google Chrome, FireFox, Edge (👀)... 

2. Navigate to: http://localhost:5162/swagger
    
3. You should see a screen like so: 

![something](/images/swagger-local.png)

> **NOTE** Swagger is a graphical UI to help explore REST APIs. You can find out more at the [swagger](https://swagger.io/) website

4. Click on each of the APIs and try out the calls. 

### Deploying and Testing via Podman-Compose

Thats cool and everything but you're here to run this as container, and more importantly orchestrated via a simple `compose.yaml` file. 

But first!! Lets have a look at our applications `Containerfile` to see how we go about creating a container image for our application. 

> **NOTE** As you'd expect we'll be using a pre-built image for the MongoDB container. 

##### Containerfile  

The `Containerfile` is a multi-stage container build and is broken down into 2 stages: 

1. Firstly, we import our source code into the MS DotNet SDK7 image, specifically into the `/source` folder. At which point we `dotnet restore` (download dependencies) and then build/publish our application for a linux based architecture. 

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:7.0 as build

# Copy csproj and restore as distinct layers 
WORKDIR /source
COPY *.csproj .
RUN dotnet restore --use-current-runtime

# Copy the rest of the project and build binaries 
COPY . .
RUN dotnet publish -c Release -o /app --os linux --arch x64 --no-cache
```

2. The second part of our `Containerfile` takes the compiled dotnet bianries and places these into our runtime container image (MS ASPNET7) under the `/app` folder. We install cURL for testing purposes and expose port 80, which is the default port for the published binary. Finally we start the application by executing the compiled `BookStoreAPI.dll` file 

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:7.0 

# Install cURL to test the REST API
RUN apt-get update -yq \
    && apt-get install curl -yq

WORKDIR /app

# Copy binary to app
COPY --from=build /app .

# Define runtime parameters 
EXPOSE 80
ENV ASPNETCORE_ENVIRONMENT=Development

# start the app! 
ENTRYPOINT ["dotnet", "BookStoreAPI.dll"]
```
> **_NOTE_** We use the ENV variable `ASPNETCORE_ENVIRONMENT=Development` to load the Development profile in our app. This ensures Swagger loads up, which typically wouldnt be available in a production profile. 

##### Compose file

Just like a `docker-compose.yaml` we are using a file names `compose.yaml` instead. Firstly, for an exhaustive overview of what you can do in a compose file, please checkout the (Compose Spec)[https://github.com/compose-spec/compose-spec/blob/master/spec.md]

In our instance, our `compose.yaml` is broken down into 2 sections: 
- MongoDB Container
- BookstoreAPI Container 

###### MongoDB compose.yaml Detail

This section contains some important points to notes around the MongoDB container instance: 

```yaml
services:
    mongo:
        container_name: bookstore-mongodb
        image: mongo
        ports:
            - '27017:27017'
        restart: always
        logging:
            options:
                max-size: 1g
        environment:
            - MONGO_INITDB_ROOT_USERNAME=mongo
            - MONGO_INITDB_ROOT_PASSWORD=mongo
            - MONGO_INITDB_DATABASE=bookstore
            - MONGO_BOOKSTORE_USER=bookstore
            - MONGO_BOOKSTORE_PASSWORD=bookstore
        networks:
            - bookstore
        volumes: 
            - ./utils/mongo-init.js:/docker-entrypoint-initdb.d/mongo-init.js:ro
```

- **Env Vars**: As you can see we bootstrap the `bookstore` database and establish root username/password. We also define some custom ENV VARS (`MONGO_BOOKSTORE_*`) that we inject into our `mongo-init.js` file.
   
- **Volumes**: There is no persistence to this container, it is ephemeral! However we do mount a single file called `mongo-init.js`. This file bootstraps mongodb with: 
  - Creates the bookstore user with corresponding credentials and role.    
  - Creates the `Books` collections
  - Injects some dummy data

- **Networks**: In this example we create a dedicated podman network for our Mongo instance and application to communicate with called `bookstore-net`. This allows our containers to use the corresponding container names as DNS names 

###### BookstoreAPI compose.yaml Detail

There isnt much else to add on top of whats already been discussed in the section above: 

```yaml 
services:
    bookstore:
        container_name: bookstore_api
        image: bookstore:v1
        ports: 
            - 8000:80
        environment:
            - MONGO_HOST=bookstore-mongodb
            - MONGO_PORT=27017
            - MONGO_USERNAME=bookstore
            - MONGO_PASSWORD=bookstore
            - MONGO_DATABASE=bookstore
        networks:
            - bookstore-net
```

- **Env Vars**: We pass our database connection string details through to our application, which will override its default params `mongodb://localhost:27017/BookStore` and use `mongodb://bookstore-mongodb:27017/bookstore`. 
  
- **Ports**: We expose bookstore_api container port 80 to our local machine on port 8000, allowing us to test in the same ways above.    

##### Podman-Compose 

Enough waffling! Lets get this container party started! 

Just like docker, `up|stop|down` are the core commands you need with podman-compose. 
    - `$ podman up` : builds all assets (networks, volumes etc.) and start the containers
    - `$ podman stop` : stops all the containers, but doesnt destroy any of the built assets
    - `$ podman up` : stops all the containers and destroys all associated assets.

Before using compose, we need to build our image so that it exists in our local repository. The compose file is looking for an image tagged: `bookstore:v1` so we need to use the command: 

1. Buld the image:
```bash
$ podman build -t bookstore:v1 . 
[1/2] STEP 1/6: FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
...
[1/2] STEP 2/6: WORKDIR /source
...
[1/2] STEP 3/6: COPY *.csproj .
...
[1/2] STEP 4/6: RUN dotnet restore --use-current-runtime
...
[1/2] STEP 5/6: COPY . .
...
[1/2] STEP 6/6: RUN dotnet publish -c Release -o /app --os linux --arch x64 --no-cache
...
[2/2] STEP 1/7: FROM mcr.microsoft.com/dotnet/aspnet:7.0
[2/2] STEP 2/7: RUN apt-get update -yq && apt-get install curl -yq
...
[2/2] STEP 3/7: WORKDIR /app
...
[2/2] STEP 4/7: COPY --from=build /app .
...
[2/2] STEP 5/7: EXPOSE 80
...
[2/2] STEP 6/7: ENV ASPNETCORE_ENVIRONMENT=Development
...
[2/2] STEP 7/7: ENTRYPOINT ["dotnet", "BookstoreAPI.dll"]
...
[2/2] COMMIT weather-api:v1
...
Successfully tagged localhost/bookstore:v1
13bb081833051ea03042aa9ea84f9e556a454ba31a5502c188bed56d466b6f2a
```
As you can see, like Docker, Podman will sequentially go through each build step, so should there be any issues its easy to identify where in the image build is the problem. 

2. Check the image exists: 
```bash
podman image ls
REPOSITORY                       TAG         IMAGE ID      CREATED         SIZE
localhost/bookstore              v1          7d00bfc50eb0  8 seconds ago   261 MB
```

3. Podman Compose!!
```bash
$ podman-compose up -d 
podman-compose version: 1.0.6
['podman', '--version', '']
using podman version: 4.5.0
** excluding:  set()
['podman', 'ps', '--filter', 'label=io.podman.compose.project=bookstoreapi', '-a', '--format', '{{ index .Labels "io.podman.compose.config-hash"}}']
['podman', 'network', 'exists', 'bookstoreapi_bookstore-net']
podman run --name=bookstore-mongodb -d --label io.podman.compose.config-hash=db1723c8207a8ea75f90d83229d572a8413cbb8480bc79e94cec0c8f54978812 --label io.podman.compose.project=bookstoreapi --label io.podman.compose.version=1.0.6 --label PODMAN_SYSTEMD_UNIT=podman-compose@bookstoreapi.service --label com.docker.compose.project=bookstoreapi --label com.docker.compose.project.working_dir=C:\Users\ally_\Documents\DotNet\BookStoreAPI --label com.docker.compose.project.config_files=compose.yaml --label com.docker.compose.container-number=1 --label com.docker.compose.service=mongo -e MONGO_INITDB_ROOT_USERNAME=mongo -e MONGO_INITDB_ROOT_PASSWORD=mongo -e MONGO_INITDB_DATABASE=bookstore -e MONGO_BOOKSTORE_USER=bookstore -e MONGO_BOOKSTORE_PASSWORD=bookstore -v C:\Users\ally_\Documents\DotNet\BookStoreAPI\utils\mongo-init.js:/docker-entrypoint-initdb.d/mongo-init.js:ro --net bookstoreapi_bookstore-net --network-alias mongo --log-driver=k8s-file --log-opt=max-size=1g -p 27017:27017 --restart always mongo
e70aae314affcc1f4e85ad9996d94ded1b4af95945fcd9c43ccf9cad53f01d52
exit code: 0
['podman', 'network', 'exists', 'bookstoreapi_bookstore-net']
podman run --name=bookstore_api -d --label io.podman.compose.config-hash=db1723c8207a8ea75f90d83229d572a8413cbb8480bc79e94cec0c8f54978812 --label io.podman.compose.project=bookstoreapi --label io.podman.compose.version=1.0.6 --label PODMAN_SYSTEMD_UNIT=podman-compose@bookstoreapi.service --label com.docker.compose.project=bookstoreapi --label com.docker.compose.project.working_dir=C:\Users\ally_\Documents\DotNet\BookStoreAPI --label com.docker.compose.project.config_files=compose.yaml --label com.docker.compose.container-number=1 --label com.docker.compose.service=bookstore -e MONGO_HOST=bookstore-mongodb -e MONGO_PORT=27017 -e MONGO_USERNAME=bookstore -e MONGO_PASSWORD=bookstore -e MONGO_DATABASE=bookstore --net bookstoreapi_bookstore-net --network-alias bookstore -p 8000:80 bookstore:v1
57e085836627c8b01124239f78eab7190133b9ac533cdc54fa9ff727223306fb
exit code: 0
```

3. Check our containers are running: 
```bash
$ podman container ls
CONTAINER ID  IMAGE                           COMMAND     CREATED             STATUS             PORTS                     NAMES
e70aae314aff  docker.io/library/mongo:latest  mongod      About a minute ago  Up About a minute  0.0.0.0:27017->27017/tcp  bookstore-mongodb
57e085836627  localhost/bookstore:v1                      59 seconds ago      Up 59 seconds      0.0.0.0:8000->80/tcp      bookstore_api
```

- Delete any leftover artiburary images: `$ podman image prune --all`


##### Testing

The configuration above leaves us with a number of testing options: 

- cURL from Localhost 
- cURL from the container 
- Browser from Localhost (Swagger)

Testing is almost identical to the testing performed above, and again b/c we've mapped port **8000** to port 80 on the container we can use the browser. 

###### cURL (On Localhost)

```bash
curl http://localhost:8000/api/Books


StatusCode        : 200
StatusDescription : OK
Content           : [{"Id":"61a6058e6c43f32854e51f51","Name":"Design Patterns","Price":54.93,"Category":"Computers","Author":"Ralph
                    Johnson"},{"Id":"61a6058e6c43f32854e51f52","Name":"Clean Code","Price":43.15,"Category":...
RawContent        : HTTP/1.1 200 OK
                    Transfer-Encoding: chunked
                    Content-Type: application/json; charset=utf-8
                    Date: Tue, 02 May 2023 13:51:20 GMT
                    Server: Kestrel

                    [{"Id":"61a6058e6c43f32854e51f51","Name":"Design Pat...
Forms             : {}
Headers           : {[Transfer-Encoding, chunked], [Content-Type, application/json; charset=utf-8], [Date, Tue, 02 May 2023
                    13:51:20 GMT], [Server, Kestrel]}
Images            : {}
InputFields       : {}
Links             : {}
ParsedHtml        : System.__ComObject
RawContentLength  : 241
```

###### cURL (On Container)

Since we installed cURL via our Containerfile, we can also fire a cURL request from within the Container itself: 

```bash
$  podman exec bookstore_api curl -s http://bookstore_api/api/Books

[{"Id":"61a6058e6c43f32854e51f51","Name":"Design Patterns","Price":54.93,"Category":"Computers","Author":"Ralph Johnson"},{"Id":"61a6058e6c43f32854e51f52","Name":"Clean Code","Price":43.15,"Category":"Computers","Author":"Robert C. Martin"}]
```

> **NOTE** Since Podman uses the container name as its host name we can use `bookstore_api` in our cURL request. We could have also used `localhost` . 

###### Browser

Finally to tie this example off, we can test Swagger via our chosen browser. Simply navigate to http://localhost:8000/swagger

![image](/images/swagger-compose.png)

#### Cleanup

To go full circle here are some commands to help shutdown and cleanup after the activities above: 

- Shutdown Containers: `$ podman-compose stop` 
- Start/Restart: `$ podman-compose start|restart` 
- Stop & Remove: `$ podman-compose down`