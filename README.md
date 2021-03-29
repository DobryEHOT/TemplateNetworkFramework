# TemplateNetworkFramework
Этот фреймворк позволяет писать шаблоны команд и реализует общение клиента с сервером с помощи них.
TCP соединение.

## Оглавление
  * [NetServer](https://github.com/DobryEHOT/TemplateNetworkFramework###NetServer)
  * [NetClient](https://github.com/DobryEHOT/TemplateNetworkFramework###NetClient)
  
  * [TNFManager](https://github.com/DobryEHOT/TemplateNetworkFramework###TNFManager)
  * [LoopIterator](https://github.com/DobryEHOT/TemplateNetworkFramework###LoopIterator)
------------

### NetServer
  Данный класс предназначен для создания реализации сервера
  
  #### Свойства
  * Сlients - словарь с подключенными клиентами
  * IP - ip сервера
  * Port - port сервера
  * isWorking - bool показывающий работает ли сервер
  * serverPassword - пароль сервера
  
  #### Методы
  * StartServer() - запускает сервер
  * StopServer() - останавливает сервер
  * SendCommandFromAllClients<T>(message) - отправить команду всем клиентам. Выполняется в параллельном потоке
    * T - название команды
    * message - строка, сообщение
  
  * DisconnectClient(name) - отключить клиента от сервера

-----------
### NetClient
Данный класс предназначен для создания реализации клиента
  
  #### Свойства
  * isWorking - bool показывающий работает ли клиент
  
  #### Методы
  * Connect(ip, port) - подключает к серверу
  * Disconnect() - отключает от сервера
  * SendCommand<T>(message) - отправить команду серверу, который после отправит эту же команду всем клиентам. Выполняется в параллельном потоке
    * T - название команды
    * message - строка, сообщение
  * AbortClient() - Агрессивный обрыв связи

-----------
### TNFManager 
#### Свойства
  * Iterator - Получить экземпляр LoopIterator
  
  #### Методы
  * InitializationCustomCommand(LibraryTemplate) - Инициализация внешних библиотек реализованных на TemplateNetworkFramework
    * LibraryTemplate - класс контейнер с экземплярами команд
  * GetCustomCommands() - получить лист с нестандартными командами
-----------
### LoopIterator
  Данный класс содержит в себе задачи которые нужно решать в главном потоке. Для этого в главном Итераторе приложения необходимо вызывать метод Update().
  
  #### Методы
  * Update() - Обновить LoopIterator
