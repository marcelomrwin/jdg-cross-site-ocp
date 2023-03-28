# EmployeeQuarkusApp

This project uses Quarkus, the Supersonic Subatomic Java Framework.

If you want to learn more about Quarkus, please visit its website: https://quarkus.io/ .

## Running the application in dev mode

You can run your application in dev mode that enables live coding using:
```shell script
./mvnw compile quarkus:dev
```

> **_NOTE:_**  Quarkus now ships with a Dev UI, which is available in dev mode only at http://localhost:8080/q/dev/.

## Packaging and running the application

The application can be packaged using:
```shell script
./mvnw package
```
It produces the `quarkus-run.jar` file in the `target/quarkus-app/` directory.
Be aware that it’s not an _über-jar_ as the dependencies are copied into the `target/quarkus-app/lib/` directory.

The application is now runnable using `java -jar target/quarkus-app/quarkus-run.jar`.

If you want to build an _über-jar_, execute the following command:
```shell script
./mvnw package -Dquarkus.package.type=uber-jar
```

The application, packaged as an _über-jar_, is now runnable using `java -jar target/*-runner.jar`.

## Creating a native executable

You can create a native executable using: 
```shell script
./mvnw package -Pnative
```

Or, if you don't have GraalVM installed, you can run the native executable build in a container using: 
```shell script
./mvnw package -Pnative -Dquarkus.native.container-build=true
```

You can then execute your native executable with: `./target/EmployeeQuarkusApp-1.0.0-SNAPSHOT-runner`

If you want to learn more about building native executables, please consult https://quarkus.io/guides/maven-tooling.

## Provided Code

### RESTEasy Reactive

Easily start your Reactive RESTful Web Services

[Related guide section...](https://quarkus.io/guides/getting-started-reactive#reactive-jax-rs-resources)

### Useful commands
```
docker run -d --rm --name postgresql -e POSTGRES_USER=admin \
           -e POSTGRES_PASSWORD=password -e POSTGRES_DB=employee_db \
           -p 5432:5432 postgres:15-alpine
           
quarkus create app com.redhat.developers:EmployeeQuarkusApp

quarkus dev -DdebugHost=0.0.0.0
```

### Generate Docker images
```
./mvnw package -DskipTests -Pnative -Dquarkus.native.container-build=true
docker build -f src/main/docker/Dockerfile.native-micro -t marcelodsales/jdg-employee-quarkus .
docker push docker.io/marcelodsales/jdg-employee-quarkus
docker run --rm -p 8080:8080 -e DATABASE_URL='host.docker.internal' marcelodsales/jdg-employee-quarkus
```

### Deploy in Openshift
```
oc process --parameters postgresql-persistent -n openshift
oc new-app --template=postgresql-persistent -p POSTGRESQL_USER=admin -p POSTGRESQL_PASSWORD=password -p POSTGRESQL_DATABASE=employee_db -p POSTGRESQL_VERSION=latest -l app=jdg-employee
oc port-forward $(oc get pods -l deploymentconfig=postgresql -o=custom-columns=NAME:.metadata.name --no-headers) 5432:5432

oc new-app --name=jdg-employee-quarkus --image=marcelodsales/jdg-employee-quarkus DATABASE_USER=admin DATABASE_PASS=password DATABASE_URL=postgresql SITE=site-1 DG_HOST=dg DG_PORT=11222 -l app=jdg-employee 
oc expose service/jdg-employee-quarkus
```

## Update Openshift ImageStream
```
oc tag docker.io/marcelodsales/jdg-employee-quarkus:latest jdg-employee-quarkus:latest
```