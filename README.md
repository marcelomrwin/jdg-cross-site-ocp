# JDG Cross Site Replication Openshift


Não consegui fazer funcionar com docker, de acordo com a página https://github.com/infinispan/infinispan-images tem alguma configuração para fazer nos clientes

```
docker run --rm --name=infinispan -p 11222:11222 -e USER="user" -e PASS="pass" quay.io/infinispan/server-native:14.0
docker run -v $(pwd)/infinispan-users:/user-config -e IDENTITIES_BATCH="/user-config/identities.batch" --rm --name=infinispan -p 11222:11222 -e USER="admin" -e PASS="password" quay.io/infinispan/server:14.0 -c infinispan.xml

infinispan-cli connect -u user -p pass --trustall http://localhost:11222
```

## OCP
**Project:** jdg-cross-site

```
oc delete secret -n jdg-cross-site dg-identities-secret --ignore-not-found
oc create secret -n jdg-cross-site generic --from-file=identities.yaml dg-identities-secret
```
