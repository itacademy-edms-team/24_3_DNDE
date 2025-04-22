import React from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { updateName, updateClass, updateLevel, updateRace, updateBackstory, updateWorldview, updatePlayerName, updateExperience } from '../../store/sheet1Slice';

import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';
import { RootState } from '../../types/state';

const Sheet1Header: React.FC = () => {
  const dispatch = useAppDispatch();

  const chName = useAppSelector((state: RootState) => state.sheet1.name);
  const handleNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateName(e.target.value));
  }

  const chClass = useAppSelector((state: RootState) => state.sheet1.class);
  const handleClassChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateClass(e.target.value));
  }

  const chLevel = useAppSelector((state: RootState) => state.sheet1.level);
  const handleLevelChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateLevel(newValue));
    }
  }

  const chRace = useAppSelector((state: RootState) => state.sheet1.race);
  const handleRaceChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateRace(e.target.value));
  }

  const chBackstory = useAppSelector((state: RootState) => state.sheet1.backstory);
  const handleBackstoryChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateBackstory(e.target.value));
  }

  const chWorldView = useAppSelector((state: RootState) => state.sheet1.worldview);
  const handleWorldviewChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateWorldview(e.target.value));
  }

  const chPlayerName = useAppSelector((state: RootState) => state.sheet1.playerName);
  const handlePlayerNameChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updatePlayerName(e.target.value));
  }

  const chExperience = useAppSelector((state: RootState) => state.sheet1.experience);
  const handleExperienceChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = Number(e.target.value);
    if (!isNaN(newValue)) {
      dispatch(updateExperience(newValue));
    }
  }

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
            <Col md={2}>
              <Form.Group controlId="characterClass">
                <Form.Control
                  value={chClass}
                  onChange={handleClassChange}
                />
                <Form.Label className="text-uppercase fw-bold">Класс</Form.Label>
              </Form.Group>
            </Col>
            <Col md={2}>
              <Form.Group controlId="characterLevel">
                <Form.Control
                  type="number"
                  value={chLevel}
                  onChange={handleLevelChange}
                />
                <Form.Label className="text-uppercase fw-bold">Уровень</Form.Label>
              </Form.Group>
            </Col>
            <Col md={4}>
              <Form.Group controlId="characterRace">
                <Form.Control
                  value={chRace}
                  onChange={handleRaceChange}
                />
                <Form.Label className="text-uppercase fw-bold">Раса</Form.Label>
              </Form.Group>
            </Col>
            <Col md={4}>
              <Form.Group controlId="characterBackstory">
                <Form.Control
                  value={chBackstory}
                  onChange={handleBackstoryChange}
                />
                <Form.Label className="text-uppercase fw-bold">Предыстория</Form.Label>
              </Form.Group>
            </Col>
          </Row>
          <Row>
            <Col md={4}>
              <Form.Group controlId="characterWorldview">
                <Form.Control
                  value={chWorldView}
                  onChange={handleWorldviewChange}
                />
                <Form.Label className="text-uppercase fw-bold">Мировоззрение</Form.Label>
              </Form.Group>
            </Col>
            <Col md={4}>
              <Form.Group controlId="characterPlayerName">
                <Form.Control
                  value={chPlayerName}
                  onChange={handlePlayerNameChange}
                />
                <Form.Label className="text-uppercase fw-bold">Имя игрока</Form.Label>
              </Form.Group>
            </Col>
            <Col md={4}>
              <Form.Group controlId="characterExperience">
                <Form.Control
                  value={chExperience}
                  onChange={handleExperienceChange}
                />
                <Form.Label className="text-uppercase fw-bold">Опыт</Form.Label>
              </Form.Group>
            </Col>
          </Row>
        </Col>
      </Row>
    </Container>
  );
};

export default Sheet1Header;