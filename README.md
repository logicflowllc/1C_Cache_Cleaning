
# 1C Cache Cleaning

Утилита для очистки кэша и временных файлов платформы 1С:Предприятие. Работает только с локальными файловыми базами данных и на WEB-серверах на базе Apache.
Требуется установленный **.NET Framework 4.8**. Никакие его возможности не используются, но в последних версиях ОС Windows его предшественники не установлены "из коробки".

Чистка кэша происходит только у пользователя, запустившего программу.

**Перед использованием утилиты, необходимо сохранить все документы, сохранить результаты работы и завершить все процессы 1С:Предприятие.**

**Программа предоставляется бесплатно по принципу "AS IS" / "Как есть". Автор не несёт отвествнности за данные, утраченные в результате использования данной программы**

## Очистка кэша

Очищает весь кэш 1С из папок *Application Data* для текущего пользователя:

* %LocalAppData%\1C\1cv8
* %LocalAppData%\1C\1cv8t
* %LocalAppData%\1C\1Cv82
* %LocalAppData%\1C\1Cv82t
* %LocalAppData%\1C\1Cv83
* %AppData%\1C\1cv8
* %AppData%\1C\1cv8t
* %AppData%\1C\1Cv82
* %AppData%\1C\1Cv82t$$
* %AppData%\1C\1Cv83

Работает в двух режимах:

* **Обычная очистка** - очищает весь кэш, который не исполььзуется ни одним сеансом 1С
* **Агрессивная очистка** - принудительно закрывает все сеансы 1С и после этого очищает весь кэш локального пользователя.

## Очистка временных файлов

Происходит принудительное завершение всех процессов 1С:Предприятие, далее удаляются все временные файлы, находящихся в одной папке с файлом 1cv8.1CD.
Файлы для очистки выбираются по расширениям: ***bin, dat, cfl, log, ind, lck, lgd, 1cl, txt, tmp, lgf, lgp, cgr***.

Структура каталогов не нарушается, все внутренние каталоги остаются на месте.

## Управление WEB-сервером Apache

Добавлена возможность управления WEB-сервером Apache в независимости от версии. Программа ищет службу с именем, содержащим подстроку Apache, запоминает имя службы и дальше может управлять ей.
Данным интсрументом удобно пользоваться, если 1С развёрнута у вас на WEB-сервере.

Для управления Apache программа запросит перезапуск **от имени Администратора**.

## Мы

* Наш сайт: <https://logicflow.ru/>
* Twitter: <https://twitter.com/LogicFlowLLC>
* Instagram: <https://www.instagram.com/logic.flow/>
* YouTube: <https://www.youtube.com/c/Try2Fix>
* Telegram: <https://t.me/logicflowru>
* Try2Fix: <https://try2fixkb.ru/>
