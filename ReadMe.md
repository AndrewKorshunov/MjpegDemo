![image](/Pictures/MainWindow.png)

����� ������� ������ _Load selected_ ���������� ������� _Loading_. ������� �� ������ �� ����� ����������� ������������.  
![image](/Pictures/MainWindow_Loading.png)

![image](/Pictures/CodeStructure.png)

### �����������:  
- �� Mjpeg ������ �������� ������ ��������.
- ��� Mjpeg ����� - ��� ����������� �����, ��� ��� ����� ������� ������ �������� ������ �����.
- ������ ���������� ����� ������� � ������.
- ������� ���������������� ������ ������������, ����� �� ��������� �� null.
- ��������� ���������, �������� ����� XML �������, �������� � App.config.

### ����������:
- ������� ��������� ����� ��������.
- ������� �������� � ����� � ������� ������, ���� ��� � �������, ������ ��� �����������. ����� ������ �� ������ ��������, � ������� byte[].
- ���������� ����������� ������ ����� �������� XML ������� (�������� �������� ������� ��� ������), ������ ���������� �����.
- ViewModel ���������� ���������� �������. ����� ��������� ��� ������.
- ������ �� ����� �������� ������ �� �����������, � ������ ������������. ����� �������� Command ����� � ������������ CommandManager.InvalidateRequerySuggested, �� ��� ��������� �� �����.
- ~~���������� �� ������� ����������, ����� ������ ��������� UI �����.~~ ������������, ���������� ����������, ���������� ������ ����������� ���� ��� �����������.

- ������ bytesRead ������ <100, ����� ����� ������� ������? 
	������� ����� 3�, ������� ��� ���� ������ 2�, �� ��� �� ������ ������
	�� ������������������.