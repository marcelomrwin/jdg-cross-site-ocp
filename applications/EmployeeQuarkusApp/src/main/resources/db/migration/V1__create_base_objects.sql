SELECT 'CREATE DATABASE employee_db' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'employee_db');

CREATE SEQUENCE IF NOT EXISTS department_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

CREATE SEQUENCE IF NOT EXISTS employee_id_seq
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 9223372036854775807
    CACHE 1;

CREATE TABLE IF NOT EXISTS department
(
    departmentid bigint NOT NULL,
    departmentname character varying(255) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT department_pkey PRIMARY KEY (departmentid),
    CONSTRAINT uk_departmentname UNIQUE (departmentname)
);

CREATE TABLE IF NOT EXISTS employee
(
    employeeid bigint NOT NULL,
    createby character varying(255) COLLATE pg_catalog."default",
    createdate timestamp without time zone,
    department bigint,
    designation character varying(255) COLLATE pg_catalog."default",
    fullname character varying(255) COLLATE pg_catalog."default",
    updatedby character varying(255) COLLATE pg_catalog."default",
    updateddate timestamp without time zone,
    uuid character varying(255) COLLATE pg_catalog."default",
    version integer NOT NULL,
    CONSTRAINT employee_pkey PRIMARY KEY (employeeid),
    CONSTRAINT uk_uuid UNIQUE (uuid)
);

ALTER TABLE employee
    ADD CONSTRAINT fk_department FOREIGN KEY (department) REFERENCES department (departmentid);