package com.redhat.developers.rest;

import java.net.URI;
import java.util.List;
import java.util.Optional;
import java.util.Set;

import javax.inject.Inject;
import javax.persistence.EntityNotFoundException;
import javax.validation.Valid;
import javax.validation.constraints.NotNull;
import javax.ws.rs.Consumes;
import javax.ws.rs.DELETE;
import javax.ws.rs.GET;
import javax.ws.rs.OPTIONS;
import javax.ws.rs.POST;
import javax.ws.rs.PUT;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.Context;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;
import javax.ws.rs.core.UriInfo;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.redhat.developers.exception.EntityOutdatedException;
import com.redhat.developers.exception.ServiceException;
import com.redhat.developers.model.Employee;
import com.redhat.developers.service.EmployeeService;
import lombok.extern.slf4j.Slf4j;
import org.eclipse.microprofile.openapi.annotations.enums.SchemaType;
import org.eclipse.microprofile.openapi.annotations.media.Content;
import org.eclipse.microprofile.openapi.annotations.media.Schema;
import org.eclipse.microprofile.openapi.annotations.parameters.Parameter;
import org.eclipse.microprofile.openapi.annotations.responses.APIResponse;

@Path("/api/employees")
@Slf4j
@Produces(MediaType.APPLICATION_JSON)
public class EmployeeResource {
    @Inject
    ObjectMapper objectMapper;

    @Inject
    EmployeeService employeeService;

    @GET
    public Response getEmployees() {
        List<Employee> employeeList = employeeService.listEmployees();
        return Response.ok(employeeList).build();
    }

    @GET
    @Path("/{employeeId}")
    public Response getEmployeeById(@NotNull Long employeeId) {
        Optional<Employee> employee = employeeService.getEmployeeById(employeeId);
        if (!employee.isPresent())
            return Response.status(Response.Status.NOT_FOUND).build();
        return Response.ok(employee.get()).build();
    }

    @APIResponse(responseCode = "201", description = "Employee Created", content = @Content(mediaType = MediaType.APPLICATION_JSON, schema = @Schema(type = SchemaType.OBJECT, implementation = Employee.class)))
    @APIResponse(responseCode = "400", description = "Invalid Employee", content = @Content(mediaType = MediaType.APPLICATION_JSON))
    @APIResponse(responseCode = "400", description = "Employee already exists for employeeId", content = @Content(mediaType = MediaType.APPLICATION_JSON))
    @POST
    @Consumes(MediaType.APPLICATION_JSON)
    public Response saveEmployee(@NotNull @Valid Employee employee, @Context UriInfo uriInfo) {
        try {
            employeeService.saveEmployee(employee);
        } catch (EntityOutdatedException eoe) {
            return Response.status(Response.Status.BAD_REQUEST).entity(eoe.getMessage()).build();
        } catch (Exception e) {
            log.error("Fail saving employee", e);
            return Response.serverError().entity(e).build();
        }
        URI uri = uriInfo.getAbsolutePathBuilder().path(Long.toString(employee.getEmployeeId())).build();
        return Response
                .created(uri)
                .entity(employee)
                .build();
    }

    @APIResponse(responseCode = "200", description = "Employee updated", content = @Content(mediaType = MediaType.APPLICATION_JSON, schema = @Schema(type = SchemaType.OBJECT, implementation = Employee.class)))
    @APIResponse(responseCode = "400", description = "Invalid Employee", content = @Content(mediaType = MediaType.APPLICATION_JSON))
    @APIResponse(responseCode = "400", description = "Employee object does not have employeeId", content = @Content(mediaType = MediaType.APPLICATION_JSON))
    @APIResponse(responseCode = "400", description = "Path variable employeeId does not match Employee.employeeId", content = @Content(mediaType = MediaType.APPLICATION_JSON))
    @APIResponse(responseCode = "404", description = "No Employee found for employeeId provided", content = @Content(mediaType = MediaType.APPLICATION_JSON))
    @PUT
    @Path("/{employeeId}")
    @Consumes(MediaType.APPLICATION_JSON)
    public Response updateEmployee(
            @Parameter(name = "employeeId", required = true) @PathParam("employeeId") Long employeeId,
            @NotNull @Valid Employee employee) {
        try {
            Employee updatedEmployee = employeeService.updateEmployee(employee);
            return Response
                    .ok(updatedEmployee)
                    .build();
        } catch (EntityOutdatedException eoe) {
            return Response.status(Response.Status.BAD_REQUEST).entity(eoe.getMessage())
                    .type(MediaType.TEXT_PLAIN_TYPE)
                    .build();
        } catch (Exception e) {
            log.error("Fail updating employee", e);
            return Response.serverError().entity(e.getMessage()).build();
        }
    }

    @DELETE
    @Path("/{employeeId}")
    public Response removeEmployee(
            @Parameter(name = "employeeId", required = true) @PathParam("employeeId") Long employeeId) {
        try {
            employeeService.deleteEmployee(employeeId);
        } catch (Exception e) {
            log.error("Fail updating employee", e);
            return Response.serverError().entity(e).build();
        }
        return Response.noContent().build();
    }

    @PUT
    @Path("/fromcache/{employeeId}")
    public Response updateEmployeeFromCache(@PathParam("employeeId") Long employeeId) {
        try {
            employeeService.updateEmployeeFromCache(employeeId);
        } catch (ServiceException | EntityNotFoundException se) {
            return Response.status(Response.Status.BAD_REQUEST).entity(se.getMessage()).build();
        }
        return Response.ok("Employee Updated successfully!").build();
    }

    @POST
    @Path("/fromcache/{uuid}")
    public Response importEmployeeFromCache(@PathParam("uuid") String uuid) {
        try {
            Long employeeId = employeeService.importEmployeeFromCache(uuid);
            return Response.status(Response.Status.CREATED).entity("Employee " + employeeId + " Updated successfully!")
                    .build();
        } catch (ServiceException se) {
            return Response.status(Response.Status.BAD_REQUEST).entity(se.getMessage()).build();
        }
    }

    @OPTIONS
    public Response getAllEmployeesKeysInCache() {
        try {
            Set<String> keys = employeeService.GetAllEmployeesKeysInCache();
            return Response.ok(keys).build();
        } catch (ServiceException se) {
            return Response.status(Response.Status.BAD_REQUEST).entity(se.getMessage()).build();
        }
    }

    @GET
    @Path("/uuid/{uuid}")
    public Response findEmployeeByUUID(@NotNull @PathParam("uuid") String uuid) {
        try {

            Optional<Employee> employee = employeeService.getEmployeeByUUID(uuid);
            if (!employee.isPresent())
                return Response.status(Response.Status.NOT_FOUND).build();

            return Response.ok(employee.get()).build();
        } catch (ServiceException se) {
            return Response.status(Response.Status.BAD_REQUEST).entity(se.getMessage()).build();
        }
    }
}
