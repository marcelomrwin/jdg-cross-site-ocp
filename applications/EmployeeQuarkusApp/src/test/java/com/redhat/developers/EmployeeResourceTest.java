package com.redhat.developers;

import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.List;
import java.util.UUID;

import javax.inject.Inject;

import com.redhat.developers.cache.DataGridRestClient;
import com.redhat.developers.model.Employee;
import com.redhat.developers.rest.EmployeeResource;
import io.quarkus.test.common.http.TestHTTPEndpoint;
import io.quarkus.test.junit.QuarkusTest;
import io.restassured.http.ContentType;
import io.restassured.response.Response;
import org.eclipse.microprofile.config.inject.ConfigProperty;
import org.eclipse.microprofile.rest.client.inject.RestClient;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;

import static io.restassured.RestAssured.given;
import static org.assertj.core.api.Assertions.assertThat;

@QuarkusTest
@TestHTTPEndpoint(EmployeeResource.class)
public class EmployeeResourceTest {

    @ConfigProperty(name = "quarkus.infinispan.cache")
    String cacheName;

    @Inject
    @RestClient
    DataGridRestClient dataGridRestClient;

    @Test
    public void getAll() {
        Response response = given()
                .when().get()
                .andReturn();

        response
                .then()
                .statusCode(200);

        List<Employee> employees = response.jsonPath().getList(".", Employee.class);
        Assertions.assertFalse(employees.isEmpty());
    }

    @Test
    public void saveEmployee() {
        Employee savedEmployee = saveAndReturnEmployee();
        assertThat(savedEmployee.getEmployeeId()).isNotNull();
    }

    @Test
    public void updateEmployee() {

        Employee employee = saveAndReturnEmployee();

        Employee savedEmployee = given()
                .when()
                .get("/{employeeId}", employee.getEmployeeId())
                .then()
                .statusCode(javax.ws.rs.core.Response.Status.OK.getStatusCode())
                .extract()
                .as(Employee.class);
        assertThat(savedEmployee).isNotNull();
        assertThat(savedEmployee.getVersion()).isEqualTo(1);

        savedEmployee.setFullName("Updated User Test Name");

        Employee updatedEmployee = given()
                .contentType(ContentType.JSON)
                .body(savedEmployee)
                .put("/{employeeId}", savedEmployee.getEmployeeId())
                .then()
                .statusCode(javax.ws.rs.core.Response.Status.OK.getStatusCode())
                .extract().as(Employee.class);

        assertThat(updatedEmployee.getCreateBy()).isEqualTo(savedEmployee.getCreateBy());
        assertThat(updatedEmployee.getCreateDate()).isEqualTo(savedEmployee.getCreateDate());
        assertThat(updatedEmployee.getVersion()).isGreaterThan(savedEmployee.getVersion());
    }

    @Test
    public void listCacheEvents() {
        given().when().get("/cache/events").then().statusCode(javax.ws.rs.core.Response.Status.OK.getStatusCode());
    }

    @Test
    public void deleteEmployee() {
        Employee savedEmployee = saveAndReturnEmployee();

        given().when().delete("/{employeeId}", savedEmployee.getEmployeeId()).then().statusCode(javax.ws.rs.core.Response.Status.NO_CONTENT.getStatusCode());

        given()
                .when()
                .get("/{employeeId}", savedEmployee.getEmployeeId())
                .then()
                .statusCode(javax.ws.rs.core.Response.Status.NOT_FOUND.getStatusCode());
    }

    @Test
    public void trySaveOutdatedEntity() {
        Employee savedEmployee = saveAndReturnEmployee();
        assertThat(savedEmployee.getEmployeeId()).isNotNull();
        assertThat(savedEmployee.getUuid()).isNotNull();

        Employee cacheEmployee = dataGridRestClient.getEmployeeFromCache(cacheName, savedEmployee.getUuid());
        assertThat(cacheEmployee).isNotNull();
        assertThat(cacheEmployee.getEmployeeId()).isEqualTo(savedEmployee.getEmployeeId());
        assertThat(cacheEmployee.getVersion()).isNotNull();
        assertThat(cacheEmployee.getVersion()).isGreaterThan(0);

        cacheEmployee.setVersion(2);
        dataGridRestClient.updateEmployeeInCache(cacheName, cacheEmployee.getUuid(), cacheEmployee);

        Response response = given()
                .contentType(ContentType.JSON)
                .body(savedEmployee)
                .put("/{employeeId}", savedEmployee.getEmployeeId())
                .thenReturn();

        response.then().statusCode(javax.ws.rs.core.Response.Status.BAD_REQUEST.getStatusCode());

        String responseString = response.body().asString();
        System.out.println(responseString);

        assertThat(responseString.contains("The local version of employee " + savedEmployee.getUuid() + " is out of date"));

    }

    private Employee saveAndReturnEmployee() {
        Employee savedEmployee = given()
                .contentType(ContentType.JSON)
                .body(getEmployee())
                .post()
                .then()
                .statusCode(javax.ws.rs.core.Response.Status.CREATED.getStatusCode())
                .extract()
                .as(Employee.class);
        return savedEmployee;
    }

    private Employee getEmployee() {
        Employee employee = Employee.builder()
                .fullName("Employee Test")
                .department(1L)
                .designation("Designation Test")
                .createBy("JUnit Test")
                .createDate(LocalDateTime.now(ZoneId.of("UTC")))
                .uuid(UUID.randomUUID().toString())
                .build();
        return employee;
    }

}