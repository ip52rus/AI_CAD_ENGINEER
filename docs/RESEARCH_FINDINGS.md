# Research Findings

## Краткий вывод

Эксперимент разделился на два разных вопроса.

### Вопрос 1

Можно ли дать внешней программе/LLM глубокий управляемый доступ к Autodesk Inventor?

**Да.**

### Вопрос 2

Достаточно ли этого, чтобы LLM стабильно и автономно выпускала чертежи человеческого качества для разных классов изделий?

**Эксперименты этого не подтвердили.**

## Что работало хорошо

### CAD connectivity

COM/API integration оказалась надёжной и управляемой.

### Factual model reading

Система хорошо извлекала:

- document structure;
- geometry;
- parameters;
- BRep;
- feature history;
- assembly occurrences;
- drawing objects.

### Targeted capability development

Capability Audit → minimum implementation → E2E оказался эффективным способом развивать CAD runtime.

### BRep reasoning

BRep помогал там, где feature history была неполной или вводила в заблуждение.

Примеры:

- mirrored generated parts;
- extrusion-cut holes;
- Frame Generator members после trim/split.

### External reasoning

LLM неплохо:

- строила factual dossier;
- сравнивала manufacturing-equivalent детали;
- классифицировала geometry;
- формировала document hierarchy;
- находила противоречия.

## Что работало нестабильно

### Main-view selection

«Информативность» вида не равна инженерной пригодности.

### Dimension completeness

Размер может быть геометрически корректным, но производственно бессмысленным; обратное тоже верно.

### Layout

Наличие координат и bounding boxes не заменяет визуальную композицию.

### Generalization

Хороший результат на валу не переносился автоматически на сварную сборку.

### Normal control

Формальная проверка ЕСКД зависит не только от геометрии, но и от design intent, назначения документа и производственного процесса.

## Важные конкретные наблюдения

### HoleFeature is not the hole

В Benchmark #2 реальные болтовые отверстия были сделаны Extrude Cut.

Вывод:

> semantic interpretation нельзя строить только по feature type.

### B_L is not final cut length

Для Frame Generator элементов параметр `B_L` в ряде случаев отличался от final BRep extent из-за miter/split/cut.

Вывод:

> manufacturing dimensions должны выводиться из конечной геометрии, а не из одного параметра.

### Assembly tree is not manufacturing hierarchy

Четыре реальные сварные единицы кресла не совпадали с native assembly tree.

Вывод:

> технологическая иерархия — engineering knowledge, а не гарантированное свойство CAD structure.

### More API does not equal better drawing

К концу эксперимента capability surface была уже очень широкой.

Ограничение оставалось в reasoning/visual judgement.

## Почему reference-aided planning был полезен

Reference drawings заметно улучшили:

- hierarchy;
- выбор локальных деталей;
- распределение информации по документам;
- понимание manufacturing conventions.

Но reference set не решил автоматически:

- конкретную размерную архитектуру;
- layout;
- production intent;
- visual cleanliness.

## Почему проект остановлен

Продолжение было технически возможно.

Stop decision был экономическим и исследовательским:

- каждый новый класс изделия требовал нового глубокого цикла;
- качество зависело от prompt/review усилий;
- human review оставался обязательным;
- выигрыш относительно работы опытного конструктора не был доказан.

## Что можно использовать дальше

Самые перспективные сценарии на базе результатов:

1. **Ask the model** — natural-language interrogation модели/сборки.
2. **Drawing review** — factual checks уже сделанного человеком чертежа.
3. **Fabrication extraction** — профили, длины, cuts, holes.
4. **Batch CAD** — repetitive Inventor operations.
5. **Supervised assistant** — пользователь принимает решение, AI исполняет локальную операцию.

## Что нельзя утверждать

Результаты проекта не доказывают, что:

- autonomous drafting невозможно в принципе;
- другая модель/подход не сможет решить задачу;
- Autodesk API является ограничивающим фактором.

Они показывают только то, что исследованный подход не обеспечил требуемое качество и обобщение в разумном workflow.
