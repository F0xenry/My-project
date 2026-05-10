-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Хост: 127.0.0.1:3307
-- Время создания: Май 10 2026 г., 20:00
-- Версия сервера: 10.4.32-MariaDB
-- Версия PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- База данных: `myapp_db`
--

-- --------------------------------------------------------

--
-- Структура таблицы `routes`
--

CREATE TABLE `routes` (
  `id` int(11) NOT NULL,
  `departure_city` varchar(100) NOT NULL,
  `arrival_city` varchar(100) NOT NULL,
  `distance_km` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Дамп данных таблицы `routes`
--

INSERT INTO `routes` (`id`, `departure_city`, `arrival_city`, `distance_km`) VALUES
(1, 'Шарыпово', 'Красноярск', NULL),
(2, 'Красноярск', 'Шарыпово', NULL);

-- --------------------------------------------------------

--
-- Структура таблицы `tickets`
--

CREATE TABLE `tickets` (
  `id` int(11) NOT NULL,
  `user_id` int(11) NOT NULL,
  `trip_id` int(11) NOT NULL,
  `passenger_name` varchar(150) NOT NULL,
  `quantity` int(11) NOT NULL DEFAULT 1,
  `total_price` decimal(10,2) NOT NULL,
  `purchase_date` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Дамп данных таблицы `tickets`
--

INSERT INTO `tickets` (`id`, `user_id`, `trip_id`, `passenger_name`, `quantity`, `total_price`, `purchase_date`) VALUES
(1, 1, 35, 'Рыбаков Эдуард Сергеевич', 1, 1500.00, '2026-05-11 00:29:42'),
(2, 1, 40, 'Рыбаков Эдуард Сергеевич', 1, 1500.00, '2026-05-11 00:31:39'),
(3, 1, 40, 'Рыбаков Эдуард Сергеевич', 1, 1500.00, '2026-05-11 00:35:42'),
(4, 1, 40, 'Рыбаков Эдуард Сергеевич', 1, 1500.00, '2026-05-11 00:38:03'),
(5, 1, 40, 'Рыбаков Эдуард Сергеевич', 1, 1500.00, '2026-05-11 00:40:44');

-- --------------------------------------------------------

--
-- Структура таблицы `trips`
--

CREATE TABLE `trips` (
  `id` int(11) NOT NULL,
  `route_id` int(11) NOT NULL,
  `departure_time` datetime NOT NULL,
  `arrival_time` datetime NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `total_seats` int(11) NOT NULL DEFAULT 50,
  `available_seats` int(11) NOT NULL DEFAULT 50,
  `bus_number` varchar(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Дамп данных таблицы `trips`
--

INSERT INTO `trips` (`id`, `route_id`, `departure_time`, `arrival_time`, `price`, `total_seats`, `available_seats`, `bus_number`) VALUES
(35, 1, '2026-05-11 02:00:00', '2026-05-11 06:30:00', 1500.00, 20, 19, 'A-101'),
(36, 1, '2026-05-11 05:00:00', '2026-05-11 09:30:00', 1500.00, 20, 19, 'A-102'),
(37, 1, '2026-05-11 08:00:00', '2026-05-11 12:30:00', 1500.00, 20, 20, 'A-103'),
(38, 1, '2026-05-11 10:00:00', '2026-05-11 14:30:00', 1500.00, 20, 20, 'A-104'),
(39, 1, '2026-05-11 13:00:00', '2026-05-11 17:30:00', 1500.00, 20, 20, 'A-105'),
(40, 1, '2026-05-12 02:00:00', '2026-05-12 06:30:00', 1500.00, 20, 16, 'A-101'),
(41, 1, '2026-05-12 05:00:00', '2026-05-12 09:30:00', 1500.00, 20, 20, 'A-102'),
(42, 1, '2026-05-12 08:00:00', '2026-05-12 12:30:00', 1500.00, 20, 20, 'A-103'),
(43, 1, '2026-05-12 10:00:00', '2026-05-12 14:30:00', 1500.00, 20, 20, 'A-104'),
(44, 1, '2026-05-12 13:00:00', '2026-05-12 17:30:00', 1500.00, 20, 20, 'A-105'),
(45, 1, '2026-05-13 02:00:00', '2026-05-13 06:30:00', 1500.00, 20, 20, 'A-101'),
(46, 1, '2026-05-13 05:00:00', '2026-05-13 09:30:00', 1500.00, 20, 20, 'A-102'),
(47, 1, '2026-05-13 08:00:00', '2026-05-13 12:30:00', 1500.00, 20, 20, 'A-103'),
(48, 1, '2026-05-13 10:00:00', '2026-05-13 14:30:00', 1500.00, 20, 20, 'A-104'),
(49, 1, '2026-05-13 13:00:00', '2026-05-13 17:30:00', 1500.00, 20, 20, 'A-105'),
(50, 1, '2026-05-14 02:00:00', '2026-05-14 06:30:00', 1500.00, 20, 20, 'A-101'),
(51, 1, '2026-05-14 05:00:00', '2026-05-14 09:30:00', 1500.00, 20, 20, 'A-102'),
(52, 1, '2026-05-14 08:00:00', '2026-05-14 12:30:00', 1500.00, 20, 20, 'A-103'),
(53, 1, '2026-05-14 10:00:00', '2026-05-14 14:30:00', 1500.00, 20, 20, 'A-104'),
(54, 1, '2026-05-14 13:00:00', '2026-05-14 17:30:00', 1500.00, 20, 20, 'A-105'),
(55, 1, '2026-05-15 02:00:00', '2026-05-15 06:30:00', 1500.00, 20, 20, 'A-101'),
(56, 1, '2026-05-15 05:00:00', '2026-05-15 09:30:00', 1500.00, 20, 20, 'A-102'),
(57, 1, '2026-05-15 08:00:00', '2026-05-15 12:30:00', 1500.00, 20, 20, 'A-103'),
(58, 1, '2026-05-15 10:00:00', '2026-05-15 14:30:00', 1500.00, 20, 20, 'A-104'),
(59, 1, '2026-05-15 13:00:00', '2026-05-15 17:30:00', 1500.00, 20, 20, 'A-105'),
(60, 1, '2026-05-16 02:00:00', '2026-05-16 06:30:00', 1500.00, 20, 20, 'A-101'),
(61, 1, '2026-05-16 05:00:00', '2026-05-16 09:30:00', 1500.00, 20, 20, 'A-102'),
(62, 1, '2026-05-16 08:00:00', '2026-05-16 12:30:00', 1500.00, 20, 20, 'A-103'),
(63, 1, '2026-05-16 10:00:00', '2026-05-16 14:30:00', 1500.00, 20, 20, 'A-104'),
(64, 1, '2026-05-16 13:00:00', '2026-05-16 17:30:00', 1500.00, 20, 20, 'A-105'),
(65, 1, '2026-05-17 02:00:00', '2026-05-17 06:30:00', 1500.00, 20, 20, 'A-101'),
(66, 1, '2026-05-17 05:00:00', '2026-05-17 09:30:00', 1500.00, 20, 20, 'A-102'),
(67, 1, '2026-05-17 08:00:00', '2026-05-17 12:30:00', 1500.00, 20, 20, 'A-103'),
(68, 1, '2026-05-17 10:00:00', '2026-05-17 14:30:00', 1500.00, 20, 20, 'A-104'),
(69, 1, '2026-05-17 13:00:00', '2026-05-17 17:30:00', 1500.00, 20, 20, 'A-105'),
(70, 2, '2026-05-11 10:00:00', '2026-05-11 14:30:00', 1500.00, 20, 20, 'B-201'),
(71, 2, '2026-05-11 11:00:00', '2026-05-11 15:30:00', 1500.00, 20, 20, 'B-202'),
(72, 2, '2026-05-11 15:00:00', '2026-05-11 19:30:00', 1500.00, 20, 20, 'B-203'),
(73, 2, '2026-05-11 16:00:00', '2026-05-11 20:30:00', 1500.00, 20, 20, 'B-204'),
(74, 2, '2026-05-11 19:00:00', '2026-05-11 23:30:00', 1500.00, 20, 20, 'B-205');

-- --------------------------------------------------------

--
-- Структура таблицы `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `login` varchar(50) NOT NULL,
  `email` varchar(100) NOT NULL,
  `phone` varchar(20) DEFAULT NULL,
  `password_hash` varchar(255) NOT NULL,
  `full_name` varchar(150) NOT NULL,
  `registration_date` datetime NOT NULL DEFAULT current_timestamp(),
  `is_active` tinyint(1) NOT NULL DEFAULT 1,
  `is_admin` tinyint(1) NOT NULL DEFAULT 0,
  `last_login` datetime DEFAULT NULL,
  `updated_at` datetime DEFAULT NULL ON UPDATE current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Дамп данных таблицы `users`
--

INSERT INTO `users` (`id`, `login`, `email`, `phone`, `password_hash`, `full_name`, `registration_date`, `is_active`, `is_admin`, `last_login`, `updated_at`) VALUES
(1, 'foxen', 'Edbopro@gmail.com', '89029417289', '$2a$12$uQe79KKLn2SabDANPq8boO8vE3lAW97n176XeZQ3GVTTsAvnqv3Gu', 'Рыбаков Эдуард Сергеевич', '2026-04-02 17:17:26', 1, 1, NULL, '2026-04-02 17:20:44'),
(2, 'client', 'foxen@gmail.ru', '89657657865', '$2a$12$MIPWmPlX9eVo8ElcyXxdc.62QIHPK1bCD.46ESK10Siwl3EH.MonO', 'Клиентов Клиент Клиентович', '2026-04-05 02:49:24', 1, 0, NULL, NULL),
(3, 'тёма', 'shura9114@gmail.com', '89233177191', '$2a$12$PFXaGWwC4GtvKntWcqma4.p6oo/a7IeqFcvpAGvD7sQEGhBd.BasC', 'Полетаев Артём Павлович', '2026-04-06 18:46:28', 1, 0, NULL, NULL),
(4, 'Client2', 'gulag@mail.ru', '88005553535', '$2a$12$Zkhv9E3IbjIss5Q.1B37QewPIGhVKj4Xr6H78owWVxVAMoIsmaXzW', 'Иванов Иван Иваночвич', '2026-04-17 13:18:31', 1, 0, NULL, NULL),
(5, 'altyshkapro228', 'top_alt_228', '89234532469', '$2a$12$9Va59CH7GgDi4fiUUcQRqe62l/1DPnKvqOu2h25RCeGmnw3STh3TO', 'Альтушкова Няша Каваевна', '2026-05-10 23:39:56', 1, 0, NULL, NULL);

--
-- Индексы сохранённых таблиц
--

--
-- Индексы таблицы `routes`
--
ALTER TABLE `routes`
  ADD PRIMARY KEY (`id`);

--
-- Индексы таблицы `tickets`
--
ALTER TABLE `tickets`
  ADD PRIMARY KEY (`id`),
  ADD KEY `user_id` (`user_id`),
  ADD KEY `trip_id` (`trip_id`);

--
-- Индексы таблицы `trips`
--
ALTER TABLE `trips`
  ADD PRIMARY KEY (`id`),
  ADD KEY `route_id` (`route_id`);

--
-- Индексы таблицы `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `login` (`login`),
  ADD UNIQUE KEY `email` (`email`),
  ADD UNIQUE KEY `phone` (`phone`),
  ADD KEY `idx_email` (`email`),
  ADD KEY `idx_phone` (`phone`),
  ADD KEY `idx_login` (`login`);

--
-- AUTO_INCREMENT для сохранённых таблиц
--

--
-- AUTO_INCREMENT для таблицы `routes`
--
ALTER TABLE `routes`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=8;

--
-- AUTO_INCREMENT для таблицы `tickets`
--
ALTER TABLE `tickets`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT для таблицы `trips`
--
ALTER TABLE `trips`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=75;

--
-- AUTO_INCREMENT для таблицы `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- Ограничения внешнего ключа сохраненных таблиц
--

--
-- Ограничения внешнего ключа таблицы `tickets`
--
ALTER TABLE `tickets`
  ADD CONSTRAINT `tickets_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`),
  ADD CONSTRAINT `tickets_ibfk_2` FOREIGN KEY (`trip_id`) REFERENCES `trips` (`id`);

--
-- Ограничения внешнего ключа таблицы `trips`
--
ALTER TABLE `trips`
  ADD CONSTRAINT `trips_ibfk_1` FOREIGN KEY (`route_id`) REFERENCES `routes` (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
