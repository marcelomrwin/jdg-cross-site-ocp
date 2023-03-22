package com.redhat.developers.exception;

import java.text.SimpleDateFormat;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.util.Objects;

import lombok.Getter;

@Getter
public class EntityOutdatedException extends RuntimeException{

    private final String uuid;
    private final  String  updatedBy ;
    private final LocalDateTime updatedDate;
    private final Integer localVersion;
    private final Integer lastVersion;

    public EntityOutdatedException(String uuid, String updatedBy, LocalDateTime updatedDate, Integer localVersion, Integer lastVersion) {
        this.uuid = uuid;
        this.updatedBy = updatedBy;
        this.updatedDate = updatedDate;
        this.localVersion = localVersion;
        this.lastVersion = lastVersion;
    }

    @Override
    public String getMessage(){
        return String.format("The local version of employee %s is out of date. The most up-to-date version is %d, updated by %s on %s and local version is %d. Please update your local version", uuid, lastVersion, Objects.requireNonNullElse(updatedBy,"TestUser"),Objects.requireNonNullElse(updatedDate,  LocalDateTime.now()) , localVersion);
    }
}
