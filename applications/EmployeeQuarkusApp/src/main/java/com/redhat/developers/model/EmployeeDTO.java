package com.redhat.developers.model;

import java.time.LocalDateTime;

import javax.validation.constraints.Min;
import javax.validation.constraints.NotEmpty;
import javax.validation.constraints.NotNull;
import javax.validation.constraints.PastOrPresent;

import lombok.Builder;
import lombok.Data;

@Builder
@Data
public class EmployeeDTO {

    @NotEmpty
    private String uuid;
    @NotEmpty
    private String fullName;
    private String designation;
    private String department;
    @PastOrPresent
    private LocalDateTime createDate;
    private String createBy;
    @PastOrPresent
    private LocalDateTime updatedDate;
    private String updatedBy;
    @NotEmpty
    @Min(1)
    private Integer version;
    @NotNull
    private STATE state;
    public enum STATE {
        CREATED, UPDATED, DELETED;
    }
}
