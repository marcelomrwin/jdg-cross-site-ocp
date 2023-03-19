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
