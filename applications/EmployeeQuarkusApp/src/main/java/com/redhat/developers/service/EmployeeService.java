package com.redhat.developers.service;

import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.List;
import java.util.Objects;
import java.util.UUID;

import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import javax.persistence.EntityManager;
import javax.persistence.EntityNotFoundException;
import javax.transaction.Transactional;
import javax.ws.rs.core.Response;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.redhat.developers.cache.DataGridRestClient;
import com.redhat.developers.exception.EntityOutdatedException;
import com.redhat.developers.exception.ServiceException;
import com.redhat.developers.model.Employee;
import lombok.extern.slf4j.Slf4j;
import org.eclipse.microprofile.config.inject.ConfigProperty;
import org.eclipse.microprofile.rest.client.inject.RestClient;

@ApplicationScoped
@Slf4j
public class EmployeeService {

    @ConfigProperty(name = "quarkus.infinispan.cache")
    String cacheName;

    @Inject
    EntityManager entityManager;

    @Inject
    @RestClient
    DataGridRestClient dataGridRestClient;

    public List<Employee> listEmployees() {
        return entityManager.createNamedQuery("Employee.findAll", Employee.class)
                .getResultList();
    }

    public Employee getEmployeeById(Long employeeId) {
        return entityManager.find(Employee.class, employeeId);
    }

    public Employee getEmployeeByUUID(String uuid) {
        return entityManager.createNamedQuery("Employee.findByUUID", Employee.class).getSingleResult();
    }

    @Transactional
    public Employee saveEmployee(Employee employee) throws Exception {

        employee.setCreateBy("QuarkusUser");
        employee.setCreateDate(LocalDateTime.now(ZoneId.of("UTC")));
        if (Objects.isNull(employee.getUuid())) {
            employee.setUuid(UUID.randomUUID().toString());
        }
        employee.setVersion(1);
        entityManager.persist(employee);
        //add to cache
        insertOrUpdateCache(employee);

        return employee;
    }

    @Transactional
    public Employee updateEmployee(Employee employee) {
        Employee dbEmployee = getEmployeeById(employee.getEmployeeId());
        if (Objects.isNull(dbEmployee))
            throw new EntityNotFoundException("Entity " + employee.getEmployeeId() + " not found in the local database");

        Employee cacheEmployee = dataGridRestClient.getEmployeeFromCache(cacheName, dbEmployee.getUuid());
        if (Objects.nonNull(cacheEmployee)) {
            if (dbEmployee.getVersion() < cacheEmployee.getVersion()) {
                throw new EntityOutdatedException(dbEmployee.getUuid(), cacheEmployee.getUpdatedBy(), cacheEmployee.getUpdatedDate(), dbEmployee.getVersion(), cacheEmployee.getVersion());
            }
        }

        employee.setUpdatedBy("QuarkusUser");
        employee.setUpdatedDate(LocalDateTime.now(ZoneId.of("UTC")));
        employee.setUuid(dbEmployee.getUuid());
        employee.setCreateDate(dbEmployee.getCreateDate());
        employee.setCreateBy(dbEmployee.getCreateBy());
        employee.setVersion(dbEmployee.getVersion() + 1);

        entityManager.merge(employee);
        entityManager.flush();

        insertOrUpdateCache(employee);

        return employee;
    }

    public void listCacheEvents() {
        Response response = dataGridRestClient.listeningToCacheEvents(cacheName);
        log.info("Response returns with status {} and content {}", response.getStatus(), response.getEntity().toString());
    }

    @Transactional
    public void deleteEmployee(Long employeeId) {
        Employee dbEmployee = entityManager.find(Employee.class, employeeId);
        if (Objects.isNull(dbEmployee))
            throw new EntityNotFoundException("Entity " + employeeId + " not found in the local database");

        entityManager.remove(dbEmployee);
    }

    private void insertOrUpdateCache(Employee employee) {
        if (!keyExists(employee.getUuid())) {
            Response response = dataGridRestClient.insertEmployeeInCache(cacheName, employee.getUuid(), employee);
            if (!checkResponseStatus(response, Response.Status.NO_CONTENT.getStatusCode())) {
                throw new ServiceException("An error occurred when trying to insert the entity into the cache.\n{}\n{}", response.getStatus(), response.getEntity());
            }
        } else {
            Response response = dataGridRestClient.updateEmployeeInCache(cacheName, employee.getUuid(), employee);
            if (!checkResponseStatus(response, Response.Status.NO_CONTENT.getStatusCode())) {
                throw new ServiceException("An error occurred when trying to update the entity into the cache.\n{}\n{}", response.getStatus(), response.getEntity());
            }
        }
    }

    private boolean keyExists(String uuid) {
        Response response = dataGridRestClient.keyExistsInCache(cacheName, uuid);
        return checkResponseStatus(response, Response.Status.OK.getStatusCode());
    }

    private boolean checkResponseStatus(Response response, int expected) {
        return response.getStatus() == expected;
    }

}
