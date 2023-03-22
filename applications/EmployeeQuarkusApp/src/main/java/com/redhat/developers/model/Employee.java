package com.redhat.developers.model;

import java.time.LocalDateTime;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.SequenceGenerator;
import javax.validation.constraints.NotEmpty;

import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

@Entity
@Data
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class Employee {

    @Id
    @SequenceGenerator(name = "employeeSeq", sequenceName = "employee_id_seq", allocationSize = 1)
    @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "employeeSeq")
    private Long employeeId;
    @Column(unique = true, updatable = false)
    private String uuid;
    @NotEmpty
    private String fullName;
    private String designation;
    private String department;
    private LocalDateTime createDate;
    private String createBy;
    private LocalDateTime updatedDate;
    private String updatedBy;
    @Column(nullable = false)
    private Integer version;
}
