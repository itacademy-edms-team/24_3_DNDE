import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router';
import { useAppSelector, useAppDispatch } from '../../hooks/index';

import { Container, Row, Col, Button, Card } from 'react-bootstrap';
import { FaTrash, FaPlus } from 'react-icons/fa';
import api from '../../api';

import { CharacterListItemDto } from '../../types/api';
import axios, { AxiosResponse } from 'axios';
import { logout } from '../../store/slices/authSlice';

export interface SheetCardProps
{
  character: CharacterListItemDto
  onCharacterClick: any
  onCharacterDelete: any
}

const SheetCard: React.FC<SheetCardProps> =
({
  character,
  onCharacterClick,
  onCharacterDelete
}) =>
{
  return (
    <Card className="w-100">
      <Card.Body
        className="d-flex gap-3 justify-content-between align-items-center"
      >
        <div className="flex-grow-1"
          style={{ cursor: "pointer" }}
          onClick={ () => onCharacterClick(character.id) }
        >
          <Card.Title>{character.name}</Card.Title>
          <Card.Title>Уровень: {character.level}</Card.Title>
          <Card.Title>Класс: {character.class}</Card.Title>
        </div>
        <FaTrash
          style={{ minWidth: "2rem", minHeight: "2rem", color: "red", cursor: "pointer" }}
          onClick={onCharacterDelete}
        />
      </Card.Body>
    </Card>
  );
}

export default SheetCard;