package com.redhat.developers.service;

import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.List;
import java.util.Objects;
import java.util.Optional;
import java.util.Set;
import java.util.UUID;

import javax.enterprise.context.ApplicationScoped;
import javax.inject.Inject;
import javax.persistence.EntityNotFoundException;
import javax.transaction.Transactional;
import javax.ws.rs.core.Response;

import com.redhat.developers.cache.DataGridRestClient;
import com.redhat.developers.exception.EntityOutdatedException;
import com.redhat.developers.exception.ServiceException;
import com.redhat.developers.model.Employee;
import com.redhat.developers.model.EmployeeDTO;
import com.redhat.developers.repository.EmployeeRepository;
import com.redhat.developers.util.EmployeeMapper;
import lombok.extern.slf4j.Slf4j;
import org.eclipse.microprofile.config.inject.ConfigProperty;
import org.eclipse.microprofile.rest.client.inject.RestClient;

@ApplicationScoped
@Slf4j
public class EmployeeService {

    @ConfigProperty(name = "quarkus.infinispan.cache")
    String cacheName;

    @Inject
    EmployeeMapper employeeMapper;

    @Inject
    EmployeeRepository employeeRepository;

    @Inject
    @RestClient
    DataGridRestClient dataGridRestClient;

    public List<Employee> listEmployees() {
        return employeeRepository.findAll().list();
    }

    public Optional<Employee> getEmployeeById(Long employeeId) {
        return employeeRepository.findByIdOptional(employeeId);
    }

    public Optional<Employee> getEmployeeByUUID(String uuid) {
        return employeeRepository.findByUUID(uuid);
    }

    @Transactional
    public Employee saveEmployee(Employee employee) throws Exception {

        employee.setCreateBy("QuarkusUser");
        employee.setCreateDate(LocalDateTime.now(ZoneId.of("UTC")));
        employee.setUpdatedBy("QuarkusUser");
        employee.setUpdatedDate(LocalDateTime.now(ZoneId.of("UTC")));
        if (Objects.isNull(employee.getUuid())) {
            employee.setUuid(UUID.randomUUID().toString());
        }
        employee.setVersion(1);
        employeeRepository.persist(employee);

        //add to cache
        EmployeeDTO employeeDTO = employeeMapper.toDTO(employee);
        employeeDTO.setState(EmployeeDTO.STATE.CREATED);
        insertOrUpdateCache(employeeDTO);

        return employee;
    }

    @Transactional
    public Employee updateEmployee(Employee employee) {
        Optional<Employee> dbEmployeeOptional = getEmployeeById(employee.getEmployeeId());
        if (!dbEmployeeOptional.isPresent())
            throw new EntityNotFoundException("Entity " + employee.getEmployeeId() + " not found in the local database");

        Employee dbEmployee = dbEmployeeOptional.get();
        EmployeeDTO cacheEmployee = dataGridRestClient.getEmployeeFromCache(cacheName, dbEmployee.getUuid());

        if (Objects.nonNull(cacheEmployee)) {
            if (dbEmployee.getVersion() < cacheEmployee.getVersion()) {
                throw new EntityOutdatedException(dbEmployee.getUuid(), cacheEmployee.getUpdatedBy(), cacheEmployee.getUpdatedDate(), dbEmployee.getVersion(), cacheEmployee.getVersion());
            }
        }

        dbEmployee.setFullName(employee.getFullName());
        dbEmployee.setDepartment(employee.getDepartment());
        dbEmployee.setDesignation(employee.getDesignation());
        dbEmployee.setVersion(dbEmployee.getVersion() + 1);
        dbEmployee.setUpdatedBy("QuarkusUser");
        dbEmployee.setUpdatedDate(LocalDateTime.now(ZoneId.of("UTC")));

        employeeRepository.persistAndFlush(dbEmployee);

        EmployeeDTO employeeDTO = employeeMapper.toDTO(dbEmployee);
        employeeDTO.setState(EmployeeDTO.STATE.UPDATED);
        insertOrUpdateCache(employeeDTO);

        return dbEmployee;
    }

    public void listCacheEvents() {
        Response response = dataGridRestClient.listeningToCacheEvents(cacheName);
        log.info("Response returns with status {} and content {}", response.getStatus(), response.getEntity().toString());
    }

    @Transactional
    public void deleteEmployee(Long employeeId) {
        Optional<Employee> dbEmployee = employeeRepository.findByIdOptional(employeeId);
        if (!dbEmployee.isPresent())
            throw new EntityNotFoundException("Entity " + employeeId + " not found in the local database");
        employeeRepository.deleteById(employeeId);

        EmployeeDTO employeeDTO = employeeMapper.toDTO(dbEmployee.get());
        dataGridRestClient.deleteEmployeeFromCache(cacheName, employeeDTO.getUuid());
    }

    @Transactional
    public void updateEmployeeFromCache(Long employeeId) {
        Optional<Employee> dbEmployeeOptional = employeeRepository.findByIdOptional(employeeId);
        if (!dbEmployeeOptional.isPresent())
            throw new ServiceException("Entity %d not found in the local database!", employeeId);

        Employee dbEmployee = dbEmployeeOptional.get();
        EmployeeDTO cacheEmployee = dataGridRestClient.getEmployeeFromCache(cacheName, dbEmployee.getUuid());

        if (Objects.nonNull(cacheEmployee)) {
            Employee employee = employeeMapper.toEntity(cacheEmployee);
            employee.setEmployeeId(employeeId);
            employeeRepository.getEntityManager().merge(employee);
            employeeRepository.getEntityManager().flush();
        } else {
            throw new ServiceException("Employee %d not found in the cache!", employeeId);
        }
    }

    @Transactional
    public Long importEmployeeFromCache(String uuid) {
        EmployeeDTO cacheEmployee = dataGridRestClient.getEmployeeFromCache(cacheName, uuid);

        if (Objects.nonNull(cacheEmployee)) {
            Employee employee = employeeMapper.toEntity(cacheEmployee);
            Optional<Employee> dbEmployeeOptional = employeeRepository.findByUUID(uuid);
            if (dbEmployeeOptional.isPresent()) {
                //update
                Employee dbEmployee = dbEmployeeOptional.get();
                employee.setEmployeeId(dbEmployee.getEmployeeId());
                employeeRepository.getEntityManager().merge(employee);
                employeeRepository.getEntityManager().flush();
            } else {
                //insert
                employeeRepository.persist(employee);
            }
            return employee.getEmployeeId();
        } else {
            throw new ServiceException("Employee %s not found in the cache!", uuid);
        }
    }

    public Set<String> GetAllEmployeesKeysInCache(){
        return dataGridRestClient.getAllKeysFromCache(cacheName,"keys");
    }

    private void insertOrUpdateCache(EmployeeDTO employeeDTO) {
        if (!keyExists(employeeDTO.getUuid())) {
            Response response = dataGridRestClient.insertEmployeeInCache(cacheName, employeeDTO.getUuid(), employeeDTO);
            if (!checkResponseStatus(response, Response.Status.NO_CONTENT.getStatusCode())) {
                throw new ServiceException("An error occurred when trying to insert the entity into the cache.\n{}\n{}", response.getStatus(), response.getEntity());
            }
        } else {
            Response response = dataGridRestClient.updateEmployeeInCache(cacheName, employeeDTO.getUuid(), employeeDTO);
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