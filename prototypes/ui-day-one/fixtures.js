/* UI-only canned read models. These are NOT GameState and contain no economy logic. */
(function () {
  "use strict";
  var base = {
    dayNumber: "01",
    date: "Вторник, 1 сентября",
    clock: "09:20",
    budget: "1 500 000 ₽",
    staffTrust: "61 / 100",
    task: "Оснащение компьютерного класса",
    visitorName: "Алина Демина",
    visitorRole: "Преподаватель проектного центра",
    quote: "«В аудитории Б-204 техника уже не справляется. Если мы ничего не решим сейчас, осенний проектный модуль придётся сокращать».",
    context: "Посетитель ожидает решения по заявке.",
    queueStatus: "НА ПРИЁМЕ",
    deadline: "2 дня",
    deadlineNote: "Заявку желательно рассмотреть до начала занятий.",
    docTitle: "Обновление компьютерного класса",
    author: "Алина Демина · проектный центр",
    amount: "240 000 ₽",
    docDeadline: "До 3 сентября",
    description: "Прошу согласовать выделение средств на обновление рабочих станций в учебной аудитории Б-204. Существующее оборудование не соответствует требованиям планируемых занятий.",
    warning: "В пакете документов нет подтверждения актуальности коммерческого предложения.",
    facts: ["Прикреплена предварительная смета поставщика.", "Актуальность цен пока не подтверждена."],
    history: ["09:20 — заявка поступила в кабинет."],
    explanation: "Изучите заявку. До подписи можно проверить смету или запросить уточнение.",
    decisionHelp: "Достоверность сметы пока не проверена. Решение может быть принято с риском.",
    resolved: false,
    dayTwo: false,
    allowInspect: true,
    allowClarify: true,
    factions: [{label:"Студенты",value:56},{label:"Сотрудники",value:61},{label:"Партнёры",value:48},{label:"Общественное доверие",value:52}]
  };
  function snapshot(patch) {
    return Object.assign({}, base, patch);
  }
  var views = {
    start: snapshot({}),
    inspected: snapshot({
      clock:"09:35",
      context:"Смета проверена. Доступно уточнение условий.",
      warning:"Проверка выявила расхождение: цены могли измениться с момента составления сметы.",
      facts:["Проверка выполнена: исходная смета составлена три недели назад.","Один из ценовых пунктов требует актуализации.","Для исправления можно запросить уточнение."],
      history:["09:20 — заявка поступила в кабинет.","09:35 — выполнена сверка сметы."],
      explanation:"Смета требует актуализации. Запросите уточнение или примите решение с известным риском.",
      decisionHelp:"Вы нашли риск устаревших цен. Можно запросить обновлённое предложение.",
      allowInspect:false
    }),
    clarified: snapshot({
      clock:"09:50",
      amount:"210 000 ₽",
      quote:"«Поставщик прислал свежие условия. Можно уложиться в меньшую сумму — теперь решение за вами».",
      context:"Запрошено уточнение, условия заявки обновлены.",
      facts:["Поступило обновлённое коммерческое предложение.","Уточнённая сумма заявки: 210 000 ₽.","Подтверждён срок поставки, оставшиеся риски минимизированы в демонстрационном сценарии."],
      history:["09:20 — заявка поступила в кабинет.","09:50 — получено обновлённое предложение: 210 000 ₽."],
      warning:"Уточнение получено. Сумма в заявке обновлена.",
      explanation:"Получен уточнённый документ. Теперь можно принять или отклонить заявку.",
      decisionHelp:"Обновлённая сумма: 210 000 ₽. Финансовый результат определяет только mock-gateway.",
      allowInspect:false,allowClarify:false
    }),
    approved: snapshot({
      clock:"10:00",budget:"1 260 000 ₽",staffTrust:"63 / 100",
      quote:"«Спасибо. Подготовим аудиторию к занятиям и отчитаемся о выполнении».",
      context:"Заявка согласована. Приём завершён.",
      queueStatus:"РЕШЕНО",deadline:"Закрыта",deadlineNote:"Решение зарегистрировано.",
      warning:"СОГЛАСОВАНО. В демонстрационном ответе списано 240 000 ₽.",
      history:["09:20 — заявка поступила в кабинет.","10:00 — согласовано: 240 000 ₽."],
      explanation:"−240 000 ₽: согласование заявки. Доверие сотрудников +2. Результат получен от mock-gateway.",
      decisionHelp:"Решение зарегистрировано. Завершите рабочий день.",
      resolved:true,factions:[{label:"Студенты",value:56},{label:"Сотрудники",value:63},{label:"Партнёры",value:48},{label:"Общественное доверие",value:52}]
    }),
    approvedClarified: snapshot({
      clock:"10:00",budget:"1 290 000 ₽",staffTrust:"63 / 100",amount:"210 000 ₽",
      quote:"«Спасибо. И за согласование, и за то, что настояли на проверке сметы».",
      context:"Уточнённая заявка согласована. Приём завершён.",
      queueStatus:"РЕШЕНО",deadline:"Закрыта",deadlineNote:"Решение зарегистрировано.",
      warning:"СОГЛАСОВАНО. В демонстрационном ответе списано 210 000 ₽.",
      facts:["Поступило обновлённое коммерческое предложение.","Итоговая сумма согласования: 210 000 ₽."],
      history:["09:20 — заявка поступила в кабинет.","09:50 — получено уточнение.","10:00 — согласовано: 210 000 ₽."],
      explanation:"−210 000 ₽: заявка после уточнения. Доверие сотрудников +2. Mock-ответ.",
      decisionHelp:"Решение зарегистрировано. Завершите рабочий день.",
      resolved:true,factions:[{label:"Студенты",value:56},{label:"Сотрудники",value:63},{label:"Партнёры",value:48},{label:"Общественное доверие",value:52}]
    }),
    rejected: snapshot({
      clock:"10:00",budget:"1 500 000 ₽",staffTrust:"58 / 100",
      quote:"«Поняла. Тогда нам придётся пересмотреть планы на осень».",
      context:"Заявка отклонена. Приём завершён.",
      queueStatus:"ОТКЛОНЕНО",deadline:"Закрыта",deadlineNote:"Решение зарегистрировано.",
      warning:"ОТКЛОНЕНО. Средства не выделены.",
      history:["09:20 — заявка поступила в кабинет.","10:00 — заявка отклонена."],
      explanation:"Бюджет не изменился. Доверие сотрудников −3. Mock-ответ.",
      decisionHelp:"Решение зарегистрировано. Завершите рабочий день.",
      resolved:true,factions:[{label:"Студенты",value:56},{label:"Сотрудники",value:58},{label:"Партнёры",value:48},{label:"Общественное доверие",value:52}]
    }),
    deferred: snapshot({
      clock:"10:00",budget:"1 500 000 ₽",staffTrust:"60 / 100",
      quote:"«Хорошо. Я принесу недостающие бумаги завтра, но времени действительно мало».",
      context:"Заявка перенесена на следующий день. Приём завершён.",
      queueStatus:"ОТЛОЖЕНО",deadline:"1 день",deadlineNote:"На завтра остаётся обязательство вернуться к вопросу.",
      warning:"ОТЛОЖЕНО. В демонстрационном сценарии расходы пока не проведены.",
      history:["09:20 — заявка поступила в кабинет.","10:00 — решение перенесено."],
      explanation:"Заявка перенесена. Доверие сотрудников −1. Mock-ответ.",
      decisionHelp:"Решение зарегистрировано. Завершите рабочий день.",
      resolved:true,factions:[{label:"Студенты",value:56},{label:"Сотрудники",value:60},{label:"Партнёры",value:48},{label:"Общественное доверие",value:52}]
    }),
    dayTwo: snapshot({
      dayNumber:"02",date:"Среда, 2 сентября",clock:"09:00",
      budget:"1 225 000 ₽",staffTrust:"63 / 100",
      visitorName:"Представитель студсовета",visitorRole:"Приём на завтра",
      quote:"«Второй день пока не реализован. Прототип демонстрирует только цикл первого рабочего дня».",
      context:"Это статичный экран-заглушка для проверки перехода дня.",
      queueStatus:"СЦЕНАРИЙ ЗАКОНЧЕН",deadline:"—",deadlineNote:"Продолжение — после интеграции Core.",
      explanation:"Первый игровой день завершён. Для другого решения обновите страницу.",
      decisionHelp:"Дальнейшие команды в прототипе не предусмотрены.",
      resolved:true,dayTwo:true,
      factions:[{label:"Студенты",value:56},{label:"Сотрудники",value:63},{label:"Партнёры",value:48},{label:"Общественное доверие",value:52}]
    })
  };
  var reports = {
    approved:{
      balance:"1 225 000 ₽",
      summary:"Вы поддержали обновление класса без дополнительной проверки.",
      ledger:[["Согласование заявки Б-204","−240 000 ₽","expense"],["Содержание корпуса (mock)","−35 000 ₽","expense"]],
      consequence:"Компьютерный класс получит финансирование. В будущем поставщик может потребовать корректировки цены. Это заготовка сюжетного последствия, а не работающий EventDirector."
    },
    approvedClarified:{
      balance:"1 255 000 ₽",
      summary:"Вы проверили условия и согласовали уточнённую смету.",
      ledger:[["Согласование уточнённой заявки","−210 000 ₽","expense"],["Содержание корпуса (mock)","−35 000 ₽","expense"]],
      consequence:"Обновление класса получило финансирование по уточнённой стоимости. Благодаря проверке меньше неопределённости. Фактические последствия впоследствии определит Core."
    },
    rejected:{
      balance:"1 465 000 ₽",
      summary:"Вы сохранили резерв, отказав в финансировании.",
      ledger:[["Согласование заявки","0 ₽","neutral"],["Содержание корпуса (mock)","−35 000 ₽","expense"]],
      consequence:"Резерв сохранён, но преподаватели разочарованы. В будущей игре это может повлиять на доступность проектных занятий."
    },
    deferred:{
      balance:"1 465 000 ₽",
      summary:"Вы перенесли решение на следующий день.",
      ledger:[["Выделение средств","0 ₽","neutral"],["Содержание корпуса (mock)","−35 000 ₽","expense"]],
      consequence:"Заявка остаётся срочной. Реальные дедлайны и отложенные последствия позднее будет обрабатывать Core, а не этот UI."
    }
  };
  var paths = {
    start:{InspectDocument:"inspected",RequestClarification:"clarified",ApproveProposal:"approved",RejectProposal:"rejected",DeferProposal:"deferred"},
    inspected:{RequestClarification:"clarified",ApproveProposal:"approved",RejectProposal:"rejected",DeferProposal:"deferred"},
    clarified:{ApproveProposal:"approvedClarified",RejectProposal:"rejected",DeferProposal:"deferred"}
  };
  window.TAU_UI_MOCK = {
    createGateway:function(){
      var current = "start";
      return {
        queryDashboard:function(){ return Promise.resolve(views[current]); },
        execute:function(command){
          if(!command || typeof command.type!=="string") return Promise.resolve({ok:false,error:"Некорректная команда."});
          if(current==="dayTwo") return Promise.resolve({ok:false,error:"День 2 не реализован в демонстрации."});
          if(command.type==="AdvanceDay") {
            if(!reports[current]) return Promise.resolve({ok:false,error:"Сначала завершите рассмотрение заявки."});
            return Promise.resolve({ok:true,view:views[current],report:reports[current]});
          }
          if(command.type==="ShowDayTwo") {
            if(!reports[current]) return Promise.resolve({ok:false,error:"Сначала завершите день."});
            var report = reports[current];
            var staffTrust = views[current].staffTrust;
            var factions = views[current].factions;
            current="dayTwo";
            return Promise.resolve({ok:true,view:Object.assign({},views.dayTwo,{budget:report.balance,staffTrust:staffTrust,factions:factions})});
          }
          if(command.documentId!=="TAU-001") return Promise.resolve({ok:false,error:"Заявка не найдена."});
          var next = paths[current] && paths[current][command.type];
          if(!next) return Promise.resolve({ok:false,error:"Действие недоступно на текущем этапе."});
          current=next;
          return Promise.resolve({ok:true,view:views[current],notice:"Ответ mock-gateway получен."});
        }
      };
    }
  };
})();
