INSERT INTO employee(employeeid, createby, createdate, department,designation,fullname,uuid,version)
VALUES
    (nextval('employee_id_seq'),'QuarkusTestApp',CURRENT_TIMESTAMP,'IT', 'Solution Architect', 'User Test Name','07478689-3d90-4b0d-ab6c-c4026db4b3c6',1),
    (nextval('employee_id_seq'), 'QuarkusTestApp', CURRENT_TIMESTAMP, 'Marketing', 'HR Manager', 'User Test Name','1eb17021-8ead-4ae9-b1a6-55cd6070aff6',1),
    (nextval('employee_id_seq'), 'QuarkusTestApp', CURRENT_TIMESTAMP, 'HR', 'Marketing Manager', 'User Test Name','a0e689cb-d2c6-428d-bd68-2d62e9116405',1);