import React from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';

import { selectCharacterName } from '../../store/selectors/sheet1Selectors';
import { selectSpellCastingAbility, selectSpellSaveDC, selectSpellAttackBonus } from '../../store/selectors/sheet3Selectors';

import { updateName } from '../../store/sheet1Slice';
import { updateSpellCastingAbility } from '../../store/sheet3Slice';

import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';
import Card from 'react-bootstrap/Card';

import CharacterSpellcastingSpellcastingAbilitySVG from './assets/CharacterSpellcastingSpellcastingAbility.svg';

import { RootState, SpellAbilityType } from '../../types/state';

const Sheet3Header: React.FC = () => {
  const dispatch = useAppDispatch();

  const chName = useAppSelector(selectCharacterName);
  const spellCastingAbility = useAppSelector(selectSpellCastingAbility);
  const spellSaveDC = useAppSelector(selectSpellSaveDC);
  const spellAttackBonus = useAppSelector(selectSpellAttackBonus);

  const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateName(e.target.value));
  };

  const handleSpellCastingAbilityChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    dispatch(updateSpellCastingAbility(e.target.value as SpellAbilityType));
  };

  return (
    <Container className="mt-3">
      <Row>
        <Col md={3} className="d-flex flex-column align-items-center justify-content-center">
          <Form.Group controlId="characterName">
            <Form.Control
              value={chName}
              onChange={handleNameChange}
            />
            <Form.Label className="text-uppercase fw-bold">Имя персонажа</Form.Label>
          </Form.Group>
        </Col>
        <Col md={1} />
        <Col md={8}>
          <Row>
            <Col>
              <Card className="p-0 m-0 border-0">
                <Card.Img src={CharacterSpellcastingSpellcastingAbilitySVG} alt="Card Image" />
                <Card.ImgOverlay className="p-0 m-0 d-flex flex-column justify-content-center text-center text-truncate">
                  <Form.Group
                    controlId="spellCastingAbility"
                    className="flex-grow-1 pt-3 px-3 d-flex flex-column"
                  >
                    <Form.Select
                      value={spellCastingAbility}
                      onChange={handleSpellCastingAbilityChange}
                      className="flex-grow-1"
                    >
                      <option value="none">Нету</option>
                      <option value="strength">Сила</option>
                      <option value="dexterity">Ловкость</option>
                      <option value="constitution">Телосложение</option>
                      <option value="intelligence">Интеллект</option>
                      <option value="wisdom">Мудрость</option>
                      <option value="charisma">Харизма</option>
                    </Form.Select>
                    <Form.Label className="fw-bold text-uppercase text-center text-wrap">
                      Характеристика заклинаний
                    </Form.Label>
                  </Form.Group>
                </Card.ImgOverlay>
              </Card>
            </Col>
            <Col>
              <Card className="p-0 m-0 border-0">
                <Card.Img src={CharacterSpellcastingSpellcastingAbilitySVG} alt="Card Image" />
                <Card.ImgOverlay className="p-0 m-0 d-flex flex-column justify-content-center text-center text-truncate">
                  <Form.Group
                    controlId="spellCastingSaveDC"
                    className="flex-grow-1 pt-3 px-3 d-flex flex-column"
                  >
                    <Form.Control
                      value={spellSaveDC}
                      readOnly
                    >
                    </Form.Control>
                    <Form.Label className="fw-bold text-uppercase text-center text-wrap">
                      Сложность броска
                    </Form.Label>
                  </Form.Group>
                </Card.ImgOverlay>
              </Card>
            </Col>
            <Col>
              <Card className="p-0 m-0 border-0">
                <Card.Img src={CharacterSpellcastingSpellcastingAbilitySVG} alt="Card Image" />
                <Card.ImgOverlay className="p-0 m-0 d-flex flex-column justify-content-center text-center text-truncate">
                  <Form.Group
                    controlId="spellCastingAttackBonus"
                    className="flex-grow-1 pt-3 px-3 d-flex flex-column"
                  >
                    <Form.Control
                      value={spellAttackBonus}
                      readOnly
                    >
                    </Form.Control>
                    <Form.Label className="fw-bold text-uppercase text-center text-wrap">
                      Бонус
                    </Form.Label>
                  </Form.Group>
                </Card.ImgOverlay>
              </Card>
            </Col>
          </Row>
        </Col>
      </Row>
    </Container>
  );
};

export default Sheet3Header;