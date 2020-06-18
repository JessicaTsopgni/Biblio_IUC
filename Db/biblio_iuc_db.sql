-- phpMyAdmin SQL Dump
-- version 4.8.3
-- https://www.phpmyadmin.net/
--
-- Hôte : 127.0.0.1:3306
-- Généré le :  mer. 17 juin 2020 à 18:59
-- Version du serveur :  5.7.23
-- Version de PHP :  7.2.10

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de données :  `biblio_iuc_db`
--

-- --------------------------------------------------------

--
-- Structure de la table `category`
--

DROP TABLE IF EXISTS `category`;
CREATE TABLE IF NOT EXISTS `category` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(50) NOT NULL,
  `description` varchar(200) DEFAULT NULL,
  `category_parent_id` int(11) DEFAULT NULL,
  `image` varchar(100) DEFAULT NULL,
  `status` smallint(6) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ix_name` (`name`),
  KEY `fk_category_category` (`category_parent_id`)
) ENGINE=InnoDB AUTO_INCREMENT=85 DEFAULT CHARSET=utf8;

--
-- Déchargement des données de la table `category`
--

INSERT INTO `category` (`id`, `name`, `description`, `category_parent_id`, `image`, `status`) VALUES
(1, 'Informatique', 'TIC, Génie logiciel, Réseau, Sécurité informatique, Télécommunication, Infographie, etc.', NULL, 'cat_f67d4926-46a0-4770-aa7b-4dbe66ab9531.jpg', 1),
(33, 'DROIT', NULL, NULL, 'cat_aa11762d-d2db-4cae-aff2-43eae7924142.jpg', 1),
(35, 'Rapports de stages d\'informatique', NULL, 1, 'cat_242bdf96-05b1-48ce-931e-a6c791a618a8.jpg', 1),
(37, 'Epreuves d\'informatique', NULL, 1, 'cat_d722321b-1899-4a8b-a389-fe3546b4d927.jpg', 1),
(40, 'Télécommunication', NULL, NULL, 'cat_d17f570f-aa5b-4efc-944e-675617374658.jpg', 1),
(42, 'Livres d\'informatique', NULL, 1, 'cat_121155d4-4a87-455e-8fcd-7e606c0e0ffe.png', 1),
(44, 'Cours d\'informatique', NULL, 1, NULL, 1),
(45, 'Epreuves de droit', NULL, 33, NULL, 1),
(46, 'Livres de droit', NULL, 33, NULL, 1),
(47, 'Rapports de stages de droit', NULL, 33, NULL, 1),
(49, 'Cours de droit', NULL, 33, NULL, 1),
(50, 'Cours de télécommunication', NULL, 40, NULL, 1),
(51, 'Rapports de stages de télécommunication', NULL, 40, NULL, 1),
(53, 'Livres de télécommunication', NULL, 40, NULL, 1),
(54, 'Epreuves de télécommunication', NULL, 40, NULL, 1),
(55, 'Chimie', NULL, NULL, 'cat_61749a2e-3bbb-454b-bbcc-7baeade388c3.jpeg', 1),
(56, 'Physiques', NULL, NULL, 'cat_fd324136-1f40-4adc-8675-f9382c94a9d8.jpg', 1),
(57, 'Mathématiques', NULL, NULL, 'cat_6523bde2-a1e7-4de1-98b9-37795c30ed26.jpg', 1),
(58, 'Agronomie', NULL, NULL, 'cat_1551f927-7786-49b5-ba7b-967c193ff609.jpg', 1),
(59, 'Ingénierie BioMedicale', NULL, NULL, 'cat_084da3e7-ad38-41f9-9d21-6b100da0944f.jpg', 1),
(60, 'Architecture et Design Industriel', NULL, NULL, 'cat_5ba10f88-c1a6-433e-8415-11889523b712.jpg', 1),
(61, 'Epreuves d\'Agronomie', NULL, 58, NULL, 1),
(62, 'Rapports de stages d\'Agronomie', NULL, 58, NULL, 1),
(63, 'Livres d\'Agronomie', NULL, 58, NULL, 1),
(64, 'Cours d\'Agronomie', NULL, 58, NULL, 1),
(65, 'Rapports de stages de physiques', NULL, 56, NULL, 1),
(66, 'Epreuves de physiques', NULL, 56, NULL, 1),
(67, 'Cours de physiques', NULL, 56, NULL, 1),
(68, 'Livres de physiques', NULL, 56, NULL, 1),
(69, 'Livres de chimie', NULL, 55, NULL, 1),
(70, 'Epreuves de chimie', NULL, 55, NULL, 1),
(71, 'Rapports de stages de chimie', NULL, 55, NULL, 1),
(72, 'Cours de chimie', NULL, 55, NULL, 1),
(73, 'Rapports de stages de Maths', NULL, 57, NULL, 1),
(74, 'Epreuves de Maths', NULL, 57, NULL, 1),
(75, 'Livres de Maths', NULL, 57, NULL, 1),
(76, 'Cours de Maths', NULL, 57, NULL, 1),
(77, 'Rapports de stages de IBM', NULL, 59, NULL, 1),
(78, 'Epreuves de IBM', NULL, 59, NULL, 1),
(79, 'Livres de IBM', NULL, 59, NULL, 1),
(80, 'Cours de IBM', NULL, 59, NULL, 1),
(81, 'Rapports de stages ADI', NULL, 60, NULL, 1),
(82, 'Epreuves ADI', NULL, 60, NULL, 1),
(83, 'Livres ADI', NULL, 60, NULL, 1),
(84, 'Cours ADI', NULL, 60, NULL, 1);

-- --------------------------------------------------------

--
-- Structure de la table `document`
--

DROP TABLE IF EXISTS `document`;
CREATE TABLE IF NOT EXISTS `document` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `code` varchar(50) NOT NULL,
  `title` varchar(100) NOT NULL,
  `subtitle` varchar(100) DEFAULT NULL,
  `authors` varchar(300) NOT NULL,
  `description` text,
  `language` varchar(50) NOT NULL,
  `publish_date` date DEFAULT NULL,
  `publisher` varchar(100) DEFAULT NULL,
  `number_of_pages` int(11) NOT NULL,
  `contributors` varchar(300) DEFAULT NULL,
  `category_id` int(11) NOT NULL,
  `image` varchar(50) DEFAULT NULL,
  `file` varchar(500) NOT NULL,
  `create_date` datetime NOT NULL,
  `status` smallint(6) NOT NULL,
  `read_count` double NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  UNIQUE KEY `ix_code` (`code`) USING BTREE,
  KEY `fk_document_category` (`category_id`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8;

--
-- Déchargement des données de la table `document`
--

INSERT INTO `document` (`id`, `code`, `title`, `subtitle`, `authors`, `description`, `language`, `publish_date`, `publisher`, `number_of_pages`, `contributors`, `category_id`, `image`, `file`, `create_date`, `status`, `read_count`) VALUES
(1, 'DOC-20204160586', 'Linq to Object', NULL, 'Zeus', 'Très bel ouvrage', 'Français', '2020-01-15', NULL, 40, NULL, 42, 'dth_0bad9e54-d36e-42c3-a4b2-72a4e025cf83.png', 'doc_b4f918e9-cbca-49fa-a1e4-0b1a2d370282.pdf', '2020-05-04 09:37:16', 1, 1),
(3, 'DOC-20205224797', 'CSharp', 'C#', 'J Martins, Kevin Clark, Rigobert Song', 'Ces critères sont dits \"In The Page\" c’est-à-dire qu’ils conservent le contenu de la page afin d’affiner leur classement, les robots utilisent également des critères \"Off The Page\" qui eux prennent en compte l’usage fait par les internautes de la page.\r\nLe robot analyse donc la pertinence des résultats qu’il fournit pour une requête donnée en calculant le temps passé par l’utilisateur sur la page proposée et son retour ou non en arrière vers le page des résultats.\r\nLes résultats proposés par le moteur sont donc issus d’une équation prenant en compte tous ces paramètres.\r\nCes critères sont dits \"In The Page\" c’est-à-dire qu’ils conservent le contenu de la page afin d’affiner leur classement, les robots utilisent également des critères \"Off The Page\" qui eux prennent en compte l’usage fait par les internautes de la page.\r\nLe robot analyse donc la pertinence des résultats qu’il fournit pour une requête donnée en calculant le temps passé par l’utilisateur sur la page proposée et son retour ou non en arrière vers le page des résultats.\r\nLes résultats proposés par le moteur sont donc issus d’une équation prenant en compte tous ces paramètres.\r\n', 'Français', '2020-02-04', 'IUC', 654, 'Jean Charles REMOND, COllins Martins, Alexandre SONG, Petit pays', 42, 'dth_4a08a8cd-47db-40ef-b662-57838d647fb0.jpg', 'doc_91df59d7-8578-4e99-b968-6db16c8b11b9.pdf', '2020-05-04 08:47:37', 1, 2),
(4, 'DOC-20205431837', 'Programmation évènementielle avec les WinForms', NULL, 'Baptiste Pesquet', NULL, 'Français', NULL, 'calibre 2.57.1 [http://calibre-ebook.com]', 39, NULL, 37, 'dth_5eec333d-13cc-45be-954e-2c38baf6b67c.png', 'doc_96b67767-60db-4f23-9156-0193bcfd5861.pdf', '2020-05-08 11:06:43', 1, 107),
(12, 'DOC-20205378865', 'corrige.dvi', 'corrige.dvi ss', 'corrige.dvi au', 'corrige.dvi desc', 'Français', '2020-05-08', 'dvips(k) 5.95a Copyright 2005 Radical Eye Software', 6, 'corrige.dvi co', 44, 'dth_913c9bfa-df35-4d83-a438-947f2ebe62b4.png', 'doc_9815f62f-35e1-452f-a7be-f564d553ce25.pdf', '2020-05-08 21:52:12', 1, 22),
(13, 'DOC-20206205159', 'Fiche d\'appressiation', NULL, 'Atonleu', NULL, 'Français', NULL, 'IUC', 1, NULL, 72, 'dth_2e75162b-5ed9-46a6-860d-79e24e275212.png', 'doc_62685a99-e34a-4369-96b5-a552882ef1e8.pdf', '2020-06-10 13:04:43', 1, 5);

-- --------------------------------------------------------

--
-- Structure de la table `user`
--

DROP TABLE IF EXISTS `user`;
CREATE TABLE IF NOT EXISTS `user` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `account` varchar(50) NOT NULL,
  `password` varchar(50) NOT NULL,
  `full_name` varchar(100) NOT NULL,
  `role` smallint(6) NOT NULL,
  `image` varchar(50) DEFAULT NULL,
  `status` smallint(6) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ix_unq_account` (`account`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

--
-- Déchargement des données de la table `user`
--

INSERT INTO `user` (`id`, `account`, `password`, `full_name`, `role`, `image`, `status`) VALUES
(1, 'admin', 'admin12345', 'Administrator', 0, 'pp_3bcd4182-b72a-49eb-a6c1-b48beeb3f6bd.png', 1),
(2, 'etudiant', '123456789', 'Toto jean de Dieu', 1, NULL, 1),
(3, 'enseignant', '123456789', 'Kankan Michel', 2, NULL, 1);

-- --------------------------------------------------------

--
-- Structure de la table `user_document`
--

DROP TABLE IF EXISTS `user_document`;
CREATE TABLE IF NOT EXISTS `user_document` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `user_id` int(11) NOT NULL,
  `document_id` int(11) NOT NULL,
  `last_page_number` int(11) NOT NULL,
  `read_date` datetime NOT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  KEY `fk_user_document_user` (`user_id`),
  KEY `fk_user_document_document` (`document_id`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8;

--
-- Déchargement des données de la table `user_document`
--

INSERT INTO `user_document` (`id`, `user_id`, `document_id`, `last_page_number`, `read_date`) VALUES
(1, 1, 3, 2, '2020-05-10 10:20:39'),
(2, 1, 4, 1, '2020-06-16 14:38:54'),
(3, 1, 12, 1, '2020-06-16 14:14:04'),
(4, 1, 13, 1, '2020-06-10 13:16:07'),
(5, 1, 4, 3, '2020-06-17 15:47:24'),
(6, 1, 13, 1, '2020-06-17 15:52:41'),
(7, 3, 3, 1, '2020-04-16 00:00:00'),
(8, 2, 13, 1, '2020-04-14 00:00:00');

--
-- Contraintes pour les tables déchargées
--

--
-- Contraintes pour la table `category`
--
ALTER TABLE `category`
  ADD CONSTRAINT `fk_category_category` FOREIGN KEY (`category_parent_id`) REFERENCES `category` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Contraintes pour la table `document`
--
ALTER TABLE `document`
  ADD CONSTRAINT `fk_document_category` FOREIGN KEY (`category_id`) REFERENCES `category` (`id`) ON DELETE NO ACTION ON UPDATE NO ACTION;

--
-- Contraintes pour la table `user_document`
--
ALTER TABLE `user_document`
  ADD CONSTRAINT `fk_user_document_document` FOREIGN KEY (`document_id`) REFERENCES `document` (`id`) ON DELETE CASCADE,
  ADD CONSTRAINT `fk_user_document_user` FOREIGN KEY (`user_id`) REFERENCES `user` (`id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
