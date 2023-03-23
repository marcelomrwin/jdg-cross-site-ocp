FROM registry.access.redhat.com/ubi8/go-toolset
COPY ./src .

RUN go mod init EmployeeGolangApp && \
    go mod tidy && \
    go build .

FROM registry.access.redhat.com/ubi8/ubi-micro
COPY --from=0 /opt/app-root/src/EmployeeGolangApp .
CMD ./EmployeeGolangApp