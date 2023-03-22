package com.redhat.developers.repository;

import java.util.Optional;

import javax.enterprise.context.ApplicationScoped;

import com.redhat.developers.model.Employee;
import io.quarkus.hibernate.orm.panache.PanacheRepositoryBase;

@ApplicationScoped
public class EmployeeRepository implements PanacheRepositoryBase<Employee,Long> {
    public Optional<Employee> findByUUID(String uuid){
        return Optional.ofNullable(find("uuid", uuid).firstResult());
    }
}
