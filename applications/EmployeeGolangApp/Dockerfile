FROM registry.access.redhat.com/ubi8/go-toolset
COPY . .

RUN go build .

FROM registry.access.redhat.com/ubi8/ubi-micro
ENV MONGO_USER 'root'
ENV MONGO_PASSWD 'password'
ENV MONGO_HOST 'mongodb'
ENV MONGO_PORT 27017
ENV SITE 'default'
ENV PORT 8080
COPY --from=0 /opt/app-root/src/EmployeeGolangApp .
EXPOSE $PORT
CMD ./EmployeeGolangApp