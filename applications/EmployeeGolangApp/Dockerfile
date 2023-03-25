FROM registry.access.redhat.com/ubi8/go-toolset
COPY . .

RUN go build .

FROM registry.access.redhat.com/ubi8/ubi-micro
ENV MONGO_USER 'root'
ENV MONGO_PASSWD 'password'
ENV MONGO_HOST 'mongodb'
ENV MONGO_PORT 27017
ENV SITE 'default'

ENV CACHE_PROTOCOL "https"
ENV CACHE_HOST "host.docker.internal"
ENV CACHE_PORT 11222
ENV CACHE_NAME "employees"
ENV CACHE_USER "admin"
ENV CACHE_PASSWD "password"

ENV PORT 8080
COPY --from=0 /opt/app-root/src/EmployeeGolangApp .
EXPOSE $PORT
CMD ./EmployeeGolangApp