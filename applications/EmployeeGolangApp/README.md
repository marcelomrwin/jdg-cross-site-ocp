# EmployeeGolangApp

## Start MongoDB
```
docker run -d --rm --name mongodb -p 27017:27017 -e MONGO_INITDB_ROOT_USERNAME=root -e MONGO_INITDB_ROOT_PASSWORD=password mongo:6
```

## Build Docker
```
docker build --tag marcelodsales/jdg-employee-golang .
```
## Docker run
```
docker run --rm -p 9000:9000 -e MONGO_HOST='host.docker.internal' marcelodsales/jdg-employee-golang
```