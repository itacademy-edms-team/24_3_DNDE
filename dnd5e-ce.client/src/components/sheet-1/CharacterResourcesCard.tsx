import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';
import Table from 'react-bootstrap/Table';
import Collapse from 'react-bootstrap/Collapse';

import { FaCog, FaTrash, FaLock, FaLockOpen, FaPlus } from 'react-icons/fa';

import { v4 as uuidv4 } from 'uuid';
import { useState, Fragment, useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';
import {  } from '../../store/sheet1Slice';
import { RootState, AttacksCardAttackEditableRowPropsType, Attack, AbilityType, AttackAbilityType, DCAbilityType, DamageAbilityType, AttacksCardGlobalDamageModifierEditableRowPropsType, GlobalDamageModifier, InventoryGold, InventoryItemEditableRowPropsType, InventoryItem } from '../../types/state';


const CharacterResourcesCard: React.FC = () => {
  // TODO: Дописать
  return (
    <>
      <Container>
        <Row>
          {0}
        </Row>
      </Container>
    </>
  );
}

export default CharacterResourcesCard;
