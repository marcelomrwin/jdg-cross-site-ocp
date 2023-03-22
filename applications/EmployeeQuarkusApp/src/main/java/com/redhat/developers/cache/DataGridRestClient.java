package com.redhat.developers.cache;

import java.util.Set;

import javax.validation.constraints.NotEmpty;
import javax.ws.rs.Consumes;
import javax.ws.rs.DELETE;
import javax.ws.rs.GET;
import javax.ws.rs.HEAD;
import javax.ws.rs.POST;
import javax.ws.rs.PUT;
import javax.ws.rs.Path;
import javax.ws.rs.PathParam;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;

import com.redhat.developers.model.EmployeeDTO;
import org.eclipse.microprofile.rest.client.inject.RegisterRestClient;

@Path("/rest/v2/caches")
@RegisterRestClient(configKey = "infinispan-rest")
@Produces(MediaType.APPLICATION_JSON)
@Consumes(MediaType.APPLICATION_JSON)
public interface DataGridRestClient {
    @GET
    @Path("/{cacheName}?action=keys")
    Set<String> getAllKeysFromCache(@NotEmpty @PathParam("cacheName") String cacheName);

    @GET
    @Path("/{cacheName}/{cacheKey}")
    EmployeeDTO getEmployeeFromCache(@NotEmpty @PathParam("cacheName") String cacheName,@NotEmpty @PathParam("cacheKey") String cacheKey);

    @HEAD
    @Path("/{cacheName}/{cacheKey}")
    Response keyExistsInCache(@NotEmpty @PathParam("cacheName") String cacheName,@NotEmpty @PathParam("cacheKey") String cacheKey);

    @POST
    @Path("/{cacheName}/{cacheKey}")
    Response insertEmployeeInCache(@NotEmpty @PathParam("cacheName") String cacheName,@NotEmpty @PathParam("cacheKey") String cacheKey, EmployeeDTO employeeDTO);

    @PUT
    @Path("/{cacheName}/{cacheKey}")
    Response updateEmployeeInCache(@NotEmpty @PathParam("cacheName") String cacheName,@NotEmpty @PathParam("cacheKey") String cacheKey, EmployeeDTO employeeDTO);

    @GET
    @Path("/{cacheName}?action=listen")
    Response listeningToCacheEvents(@NotEmpty @PathParam("cacheName") String cacheName);

    @DELETE
    @Path("/{cacheName}/{cacheKey}")
    Response deleteEmployeeFromCache(@NotEmpty @PathParam("cacheName") String cacheName,@NotEmpty @PathParam("cacheKey") String cacheKey);

}
