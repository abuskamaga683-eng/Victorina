// надписи и цвета на секторах
const prizes = [
  {
    text: "1",
    color: "hsl(197 30% 43%)",
    question: "Тип мышления, заключающийся во фрагментарном восприятии информации.Люди с таким мышлением склонны к восприятию информации отрывочно и порционно,небольшими кусками, при этом значительно страдает глубина понимания материала,а также критический подход к информации.",
	
  },
  { 
    text: "2",
    color: "hsl(173 58% 39%)",
    question: "С его помощью мошенники выуживают у пользователя данные и потом используют их в своих целях.",
	
  },
  { 
    text: "3",
    color: "hsl(43 74% 66%)",
    question: "Группы, распространяющие идею и практики причинения самому себе физического или психологического вреда.",
	
  },
  {
    text: "4",
    color: "hsl(197 30% 43%)",
    question: "Движение, ставшее популярным в США. Эти сообщества романтизируют и продвигают идею массовых убийств и, в особенности, массовых убийств среди детей и подростков в школах.",
	
  },
  { 
    text: "5",
    color: "hsl(173 58% 39%)",
    question: "Что относится к персональным данным?",
	
  },
  { 
    text: "6",
    color: "hsl(43 74% 66%)",
    question: "Совокупность методов и практик защиты от атак злоумышленников для компьютеров, серверов, мобильных устройств, электронных систем, сетей и данных."
	
  },
  {
    text: "7",
    color: "hsl(197 30% 43%)",
    question: "В цифровом мире множество неконтролируемых уведомлений,которые приходят на телефон практически ежеминутно",
	
  },
  { 
    text: "8",
    color: "hsl(173 58% 39%)",
    question: "Куда обращаться если Вы установили факты распространения детской порнографии, призывов к суициду, рекламы азартных игр (онлайн-казино) или склонения несовершеннолетних к противоправным действиям.",
  },
  { 
    text: "9",
    color: "hsl(43 74% 66%)",
    question: "Трэш-стримы. Что это и куда отправляется жалоба, при их обнаружении.",
  },
  {
    text: "10",
    color: "hsl(197 30% 43%)",
    question: "Каким термином обозначают `излишнюю открытость в социальных сетях`.",
  },
  { 
    text: "11",
    color: "hsl(173 58% 39%)",
    question: "Метод атаки или взлома путем перебора всех возможных вариантов пароля.",
  },
  { 
    text: "12",
    color: "hsl(43 74% 66%)",
    question: "Разновидность вредоносной программы, проникающая в компьютер под видом легитимного программного обеспечения",
  },
  {
    text: "13",
    color: "hsl(197 30% 43%)",
    question: "Какая статья УК РФ регламентирует наказание за незаконное собирание или распространение сведений о частной жизни лица,составляющих его личную или семейную тайну, без его согласия либо распространение этих сведений в публичном выступлении, публично демонстрирующемся произведении или средствах массовой информации ",
  },
  { 
    text: "14",
    color: "hsl(173 58% 39%)",
    question: "Перечислите правило для создания надежного пароля.",
  },
  { 
    text: "15",
    color: "hsl(43 74% 66%)",
    question: "Целенаправленно распространяемая ложная информация под видом достоверной.",
  },
  {
    text: "Блиц",
    color: "hsl(197 30% 43%)",
    question: "Могут ли завести уголовное дело за лайк в социальных сетях?",
  },
  { 
    text: "Черный ящик",
    color: "hsl(173 58% 39%)",
    question: "Единственный нормативный акт, устанавливающий преступность и наказуемость деяний на территории Российской Федерации.",
  },
];

// создаём переменные для быстрого доступа ко всем объектам на странице — блоку в целом, колесу, кнопке и язычку
const wheel = document.querySelector(".deal-wheel");
const question = document.querySelector('.spinner-text');
const spinner = wheel.querySelector(".spinner");
const trigger = wheel.querySelector(".btn-spin");
const ticker = wheel.querySelector(".ticker");
const tyty = document.querySelector(".spinner-text-b");

// на сколько секторов нарезаем круг
const prizeSlice = 360 / prizes.length;
// на какое расстояние смещаем сектора друг относительно друга
const prizeOffset = Math.floor(180 / prizes.length);
// прописываем CSS-классы, которые будем добавлять и убирать из стилей
const spinClass = "is-spinning";
const selectedClass = "selected";
// получаем все значения параметров стилей у секторов
const spinnerStyles = window.getComputedStyle(spinner);

// переменная для анимации
let tickerAnim;
// угол вращения
let rotation = 0;
// текущий сектор
let currentSlice = 0;
// переменная для текстовых подписей
let prizeNodes;

// расставляем текст по секторам
const createPrizeNodes = () => {
  // обрабатываем каждую подпись
  prizes.forEach(({ text, color, reaction}, i) => {
    // каждой из них назначаем свой угол поворота
    const rotation = ((prizeSlice * i) * -1) - prizeOffset;
    // добавляем код с размещением текста на страницу в конец блока spinner
    spinner.insertAdjacentHTML(
      "beforeend",
      // текст при этом уже оформлен нужными стилями
      `<li class="prize" data-reaction=${reaction} style="--rotate: ${rotation}deg">
        <span class="text">${text}</span>
      </li>`
    );
  });
};

// рисуем разноцветные секторы
const createConicGradient = () => {
  // устанавливаем нужное значение стиля у элемента spinner
  spinner.setAttribute(
    "style",
    `background: conic-gradient(
      from -90deg,
      ${prizes
        // получаем цвет текущего сектора
        .map(({ color }, i) => `${color} 0 ${(100 / prizes.length) * (prizes.length - i)}%`)
        .reverse()
      }
    );`
  );
};

// создаём функцию, которая нарисует колесо в сборе
const setupWheel = () => {
  // сначала секторы
  createConicGradient();
  // потом текст
  createPrizeNodes();
  // а потом мы получим список всех призов на странице, чтобы работать с ними как с объектами
  prizeNodes = wheel.querySelectorAll(".prize");
};

// определяем количество оборотов, которое сделает наше колесо
const spinertia = (min, max) => {
  min = Math.ceil(min);
  max = Math.floor(max);
  return Math.floor(Math.random() * (max - min + 1)) + min;
};

// функция запуска вращения с плавной остановкой
const runTickerAnimation = () => {
  // взяли код анимации отсюда: https://css-tricks.com/get-value-of-css-rotation-through-javascript/
  const values = spinnerStyles.transform.split("(")[1].split(")")[0].split(",");
  const a = values[0];
  const b = values[1];  
  let rad = Math.atan2(b, a);
  
  if (rad < 0) rad += (2 * Math.PI);
  
  const angle = Math.round(rad * (180 / Math.PI));
  const slice = Math.floor(angle / prizeSlice);

  // анимация язычка, когда его задевает колесо при вращении
  // если появился новый сектор
  if (currentSlice !== slice) {
    // убираем анимацию язычка
    ticker.style.animation = "none";
    // и через 10 миллисекунд отменяем это, чтобы он вернулся в первоначальное положение
    setTimeout(() => ticker.style.animation = null, 10);
    // после того, как язычок прошёл сектор - делаем его текущим 
    currentSlice = slice;
  }
  // запускаем анимацию
  tickerAnim = requestAnimationFrame(runTickerAnimation);
};

// функция выбора призового сектора
const selectPrize = () => {
  const selected = Math.floor(rotation / prizeSlice);
  prizeNodes[selected].classList.add(selectedClass);
  question.textContent = 'Сектор '+prizes[selected].text+'.';
  tyty.textContent = prizes[selected].question;
};

// отслеживаем нажатие на кнопку
trigger.addEventListener("click", () => {
  question.textContent = '';
  tyty.textContent = '';
  // делаем её недоступной для нажатия
  trigger.disabled = true;
  // задаём начальное вращение колеса
  rotation = Math.floor(Math.random() * 360 + spinertia(2000, 5000));
  // убираем прошлый приз
  prizeNodes.forEach((prize) => prize.classList.remove(selectedClass));
  // добавляем колесу класс is-spinning, с помощью которого реализуем нужную отрисовку
  wheel.classList.add(spinClass);
  // через CSS говорим секторам, как им повернуться
  spinner.style.setProperty("--rotate", rotation);
  // возвращаем язычок в горизонтальную позицию
  ticker.style.animation = "none";
  // запускаем анимацию вращение
  runTickerAnimation();
});

// отслеживаем, когда закончилась анимация вращения колеса
spinner.addEventListener("transitionend", () => {
  // останавливаем отрисовку вращения
  cancelAnimationFrame(tickerAnim);
  // получаем текущее значение поворота колеса
  rotation %= 360;
  // выбираем приз
  selectPrize();
  // убираем класс, который отвечает за вращение
  wheel.classList.remove(spinClass);
  // отправляем в CSS новое положение поворота колеса
  spinner.style.setProperty("--rotate", rotation);
  // делаем кнопку снова активной
  trigger.disabled = false;
});

// подготавливаем всё к первому запуску
setupWheel();