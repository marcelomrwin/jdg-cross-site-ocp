# EmployeeGolangApp

## Start MongoDB
```
docker run -d --rm --name mongodb -p 27017:27017 -e MONGO_INITDB_ROOT_USERNAME=root -e MONGO_INITDB_ROOT_PASSWORD=password mongo:6
```

## Build Docker
```
docker build --tag marcelodsales/jdg-employee-golang .
docker push docker.io/marcelodsales/jdg-employee-golang
```
## Docker run
```
docker run --rm -p 9000:9000 -e MONGO_HOST='host.docker.internal' marcelodsales/jdg-employee-golang
```

## Openshift Deploy
```
oc apply -f mongo-4-template.json
oc process --parameters mongo4
oc new-app --template=mongo4 -p MONGODB_USER=root -p MONGODB_PASSWORD=password -p MONGODB_DATABASE=employeedb -p VOLUME_CAPACITY=1Gi -l app=jdg-employee
oc port-forward $(oc get pods -l deploymentconfig=mongodb -o=custom-columns=NAME:.metadata.name --no-headers) 27017:27017
oc new-app --name=jdg-employee-golang --image=marcelodsales/jdg-employee-golang MONGO_USER=root MONGO_PASSWD=password MONGO_HOST=mongodb MONGO_PORT=27017 SITE=site-1 CACHE_HOST=dg -l app=jdg-employee 
oc expose service/jdg-employee-golang
```

## Update imagestream
```
oc tag docker.io/marcelodsales/jdg-employee-golang:latest jdg-employee-golang:latest
```

### If need to delete app
```
oc delete dc mongodb
oc delete svc mongodb
oc delete pvc mongodb-pvc
oc delete secret mongodb-secret

oc delete deployment jdg-employee-golang
oc delete svc jdg-employee-golang
```
