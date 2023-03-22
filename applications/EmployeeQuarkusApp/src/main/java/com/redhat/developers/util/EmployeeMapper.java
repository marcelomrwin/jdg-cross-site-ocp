package com.redhat.developers.util;

import java.util.List;

import com.redhat.developers.model.Employee;
import com.redhat.developers.model.EmployeeDTO;
import org.mapstruct.InheritInverseConfiguration;
import org.mapstruct.Mapper;
import org.mapstruct.MappingTarget;

@Mapper(componentModel = "cdi")
public interface EmployeeMapper {
    List<Employee> toEntityList(List<EmployeeDTO> entitiesDTO);

    Employee toEntity(EmployeeDTO entityDTO);

    @InheritInverseConfiguration(name = "toEntity")
    EmployeeDTO toDTO(Employee entity);

    void updateEntityFromDTO(Employee entity, @MappingTarget EmployeeDTO entityDTO);

    void updateDTOFromEntity(EmployeeDTO entityDTO, @MappingTarget Employee entity);
}
