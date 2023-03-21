package com.redhat.developers.model;

import java.time.LocalDateTime;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.NamedQueries;
import javax.persistence.NamedQuery;
import javax.persistence.QueryHint;
import javax.persistence.SequenceGenerator;
import javax.validation.constraints.NotEmpty;
import javax.validation.constraints.NotNull;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Getter;
import lombok.NoArgsConstructor;
import lombok.Setter;

@Entity
@Getter
@Setter
@Builder
@NoArgsConstructor
@AllArgsConstructor
@NamedQueries({
        @NamedQuery(name = "Employee.findAll",
                query = "SELECT e FROM Employee e ORDER BY e.fullName",
                hints = @QueryHint(name = "org.hibernate.cacheable", value = "true")),
        @NamedQuery(name = "Employee.findByUUID", query = "SELECT e FROM Employee e WHERE e.uuid = :uuid",
                hints = @QueryHint(name = "org.hibernate.cacheable", value = "true")),
})
public class Employee {

    @Id
    @SequenceGenerator(name = "employeeSeq", sequenceName = "employee_id_seq", allocationSize = 1, initialValue = 1)
    @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "employeeSeq")
    private Long employeeId;
    @Column(unique = true,updatable = false)
    private String uuid;
    @NotEmpty
    private String fullName;
    private String designation;
    private Long department;
    private LocalDateTime createDate;
    private String createBy;
    private LocalDateTime updatedDate;
    private String updatedBy;
    @Column(nullable = false)
    private Integer version;
}
