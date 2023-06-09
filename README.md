# JDG Cross Site Replication Openshift

To use the ansible playbook you need to configure variables: 
* site1_base_url: namespace . cluster domain
* site2_base_url: namespace . cluster domain
* quarkus_app:  Name of the Quarkus Application.  _Default_: jdg-employee-quarkus
* dotnet_app:   Name of the Net Core Application. _Default_: jdg-employee-dotnet
* golang_app:   Name of the Golang Application.   _Default_: jdg-employee-golang


To test local download infinispan server from https://infinispan.org/download/

## Openshift

## Create cluster infrastructure
```
oc login --token=...
oc create namespace rhdg-xsite
oc project rhdg-xsite
oc create secret -n rhdg-xsite generic --from-file=identities.yaml dg-identities-secret
cd certs
./generate-certificate.sh
oc -n rhdg-xsite create secret generic xsite-keystore "--from-file=keystore.p12=$(pwd)/dg-keystore.p12" "--from-literal=password=secret" "--from-literal=type=pkcs12"
oc -n rhdg-xsite create secret generic xsite-truststore "--from-file=truststore.p12=$(pwd)/ca-keystore.p12" "--from-literal=password=caSecret"
```

```
oc delete secret -n datagrid dg-identities-secret --ignore-not-found
oc create secret -n datagrid generic --from-file=identities.yaml dg-identities-secret
```

## Port-Forward datagrid
```
oc port-forward $(oc get pods -l app=infinispan-pod -o=custom-columns=NAME:.metadata.name --no-headers) 11222:11222
```

https://infinispan.org/docs/infinispan-operator/main/operator.html#operator
https://infinispan.org/docs/stable/titles/rest/rest.html#rest_server
http://www.mastertheboss.com/jboss-frameworks/infinispan/infinispan-restful-interface/

### Create Infinispan Cluster using Operators

#### Site 1
```yaml
apiVersion: infinispan.org/v1
kind: Infinispan
metadata:
  name: dg
  namespace: rhdg-xsite
spec:
  security:
    endpointAuthentication: true
    endpointSecretName: dg-identities-secret
  container:
    memory: 1Gi
  expose:
    type: Route
  service:
    container:
      storage: 1Gi
    sites:
      local:
        discovery:
          launchGossipRouter: true
          type: gossiprouter
        encryption:
          routerKeyStore:
            alias: dg
            secretName: xsite-keystore
          transportKeyStore:
            alias: dg
            secretName: xsite-keystore
          trustStore:
            filename: truststore.p12
            secretName: xsite-truststore
        expose:
          type: Route
        maxRelayNodes: 1
        name: site-1
      locations:
        - name: site-2
    type: DataGrid
  replicas: 1
```

#### Site 2
```yaml
apiVersion: infinispan.org/v1
kind: Infinispan
metadata:
  name: dg
spec:
  security:
    endpointSecretName: dg-identities-secret
  container:
    memory: 1Gi
  expose:
    type: Route
  service:
    container:
      storage: 1Gi
    sites:
      local:
        discovery:
          launchGossipRouter: false
          type: gossiprouter
        encryption:
          routerKeyStore:
            alias: dg
            secretName: xsite-keystore
          transportKeyStore:
            alias: dg
            secretName: xsite-keystore
          trustStore:
            filename: truststore.p12
            secretName: xsite-truststore
        expose:
          type: Route
        maxRelayNodes: 1
        name: site-2
      locations:
        - name: site-1
          url: infinispan+xsite://dg-route-site-rhdg-xsite.apps.ocp4.masales.cloud:443
    type: DataGrid
  configListener:
    enabled: true
  replicas: 1
```

#### Cache Site 1
```yaml
apiVersion: infinispan.org/v2alpha1
kind: Cache
metadata:  
  name: employees
  namespace: rhdg-xsite
spec:
  clusterName: dg
  name: employees
  template: |
    replicatedCache: 
      mode: "ASYNC"
      statistics: "true"
      encoding: 
        key: 
          mediaType: "text/plain; charset=UTF-8"
        value: 
          mediaType: "application/json; charset=UTF-8"
      locking: 
        isolation: "REPEATABLE_READ"
        acquireTimeout: "0"
      expiration: 
        lifespan: "600000"
        maxIdle: "300000"
      backups: 
        site-2: 
          backup: 
            strategy: "SYNC"
            takeOffline: 
              minWait: "120000"      
  updates:
    strategy: retain
```

#### Cache Site 2
```yaml
apiVersion: infinispan.org/v2alpha1
kind: Cache
metadata:  
  name: employees
  namespace: rhdg-xsite
spec:
  clusterName: dg
  name: employees
  template: |
    replicatedCache: 
      mode: "ASYNC"
      statistics: "true"
      encoding: 
        key: 
          mediaType: "text/plain; charset=UTF-8"
        value: 
          mediaType: "application/json; charset=UTF-8"
      locking: 
        isolation: "REPEATABLE_READ"
        acquireTimeout: "0"
      expiration: 
        lifespan: "600000"
        maxIdle: "300000"
      backups: 
        site-1: 
          backup: 
            strategy: "SYNC"
            takeOffline: 
              minWait: "120000"
  updates:
    strategy: retain
```