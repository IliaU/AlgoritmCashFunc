��������! ������ ���� ����������� � ������� ������������ ��� ������ ������, � ������� 
����� ������ � ����: ���� ������ ���� ������.


CREATE USER 'AKS1'@'CHUDAKOV' IDENTIFIED BY '123';

GRANT ALL PRIVILEGES ON * . * TO 'AKS1'@'CHUDAKOV';
FLUSH PRIVILEGES;

Create SCHEMA aks;