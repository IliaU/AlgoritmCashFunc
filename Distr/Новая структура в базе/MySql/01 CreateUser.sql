Внимание! Юзеров надо прописывать с нужными привелегиями для каждой машины, с которой 
будет доступ к базе: хоть чтение хоть запись.


CREATE USER 'AKS1'@'CHUDAKOV' IDENTIFIED BY '123';

GRANT ALL PRIVILEGES ON * . * TO 'AKS1'@'CHUDAKOV';
FLUSH PRIVILEGES;

Create SCHEMA aks;