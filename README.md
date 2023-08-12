# My First Project
Описание:
Решение состоит из 3х проектов: консольное приложение "ConsoleApp", asp.net core web api "Web", библиотека для работы с Apache Kafka "MessageBusLib". Консольное приложение каждую минуту собирает курс различных валют и записывает в Redis. Apache kafka передает данные между проектами с помощью циклов по двум топикам "response" и "request". Пользователь может увидеть средний курс за последние 10 минут, отправив соответствующие запрос в asp.net core web api. Вычисления проходят на стороне консольного приложения. Web api отправляет запрос по Apache kafka и ответ выводит пользователю. 
Управление (запрсы в asp.net core web api):
  "cadRub" - средний курс канадского доллара к рублю за последние 10 минут,
  "audRub" - средний курс австралийского доллара к рублю за последние 10 минут,
  "gbpRub" - средний курс британскиого фунта к рублю за последние 10 минут,
  "usdRub" - средний курс американского доллара к рублю за последние 10 минут,
  "eurRub" - средний курс евро к рублю за последние 10 минут.