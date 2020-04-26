-- phpMyAdmin SQL Dump
-- version 4.8.3
-- https://www.phpmyadmin.net/
--
-- Hôte : 127.0.0.1:3306
-- Généré le :  Dim 26 avr. 2020 à 19:16
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
) ENGINE=InnoDB AUTO_INCREMENT=40 DEFAULT CHARSET=utf8;

--
-- Déchargement des données de la table `category`
--

INSERT INTO `category` (`id`, `name`, `description`, `category_parent_id`, `image`, `status`) VALUES
(1, 'Informatique', 'TIC, Génie logiciel, Réseau, Sécurité informatique, Télécommunication, Infographie, etc.', NULL, NULL, 1),
(2, 'Droit', 'Droit constitutionnel, administratif, des finances publiques, civil. social, pénal, commercial, judiciaire.', 33, 'cat_5e9a5c515ab5e.png', 1),
(32, 'rbsdf', 'sDBSDF', NULL, NULL, 1),
(33, 'fdndfndn d asdcjkabjca askjcbkajsbcakjs', 'dfnDFn', NULL, NULL, 0),
(34, 'sdb', 'bdS', NULL, NULL, 1),
(35, 'jon snow', NULL, 1, NULL, 1),
(37, 'test', NULL, 1, 'cat_0a425f0d-ec0b-441a-9ac7-f6b16826c3b3.webp', 1),
(39, 'test 2', NULL, NULL, NULL, 1);

-- --------------------------------------------------------

--
-- Structure de la table `document`
--

DROP TABLE IF EXISTS `document`;
CREATE TABLE IF NOT EXISTS `document` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `isbn` varchar(50) NOT NULL,
  `title` varchar(100) NOT NULL,
  `subtitle` varchar(100) DEFAULT NULL,
  `description` varchar(500) NOT NULL,
  `language` varchar(50) NOT NULL,
  `publish_date` date NOT NULL,
  `publisher` varchar(100) NOT NULL,
  `number_of_pages` int(11) NOT NULL,
  `contributors` varchar(300) NOT NULL,
  `category_id` int(11) NOT NULL,
  `image` varchar(50) DEFAULT NULL,
  `file` varchar(500) NOT NULL,
  `status` smallint(6) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ix_isbn` (`isbn`),
  KEY `fk_document_category` (`category_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

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
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

--
-- Déchargement des données de la table `user`
--

INSERT INTO `user` (`id`, `account`, `password`, `full_name`, `role`, `image`, `status`) VALUES
(1, 'admin', 'admin12345', 'Administrator', 0, 'pp_3bcd4182-b72a-49eb-a6c1-b48beeb3f6bd.png', 1);

--
-- Contraintes pour les tables déchargées
--

--
-- Contraintes pour la table `category`
--
ALTER TABLE `category`
  ADD CONSTRAINT `fk_category_category` FOREIGN KEY (`category_parent_id`) REFERENCES `category` (`id`);

--
-- Contraintes pour la table `document`
--
ALTER TABLE `document`
  ADD CONSTRAINT `fk_document_category` FOREIGN KEY (`category_id`) REFERENCES `category` (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
