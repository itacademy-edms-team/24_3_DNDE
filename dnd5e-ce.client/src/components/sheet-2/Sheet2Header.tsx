import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { updateName } from '../../store/sheet1Slice';
import { updateAge, updateHeight, updateWeight, updateEyes, updateSkin, updateHair } from '../../store/sheet2Slice';

const Sheet2Header: React.FC = () => {
  const dispatch = useAppDispatch();
  const { name } = useAppSelector((state) => state.sheet1);
  const { age, height, weight, eyes, skin, hair } = useAppSelector((state) => state.sheet2);

  const handleNameChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateName(event.target.value));
  };

  const handleAgeChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateAge(event.target.value));
  };

  const handleHeightChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateHeight(event.target.value));
  };

  const handleWeightChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateWeight(event.target.value));
  };

  const handleEyesChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateEyes(event.target.value));
  };

  const handleSkinChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateSkin(event.target.value));
  };

  const handleHairChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    dispatch(updateHair(event.target.value));
  };

  return (
    <Container className="mt-3">
      <Row>
        <Col md={3} className="d-flex flex-column align-items-center justify-content-center">
          <Form.Group controlId="characterName" className="w-100 text-center">
            <Form.Control
              type="text"
              value={name}
              onChange={handleNameChange}
              placeholder="Введите имя персонажа"
              aria-label="Имя персонажа"
            />
            <Form.Label className="text-uppercase fw-bold">Имя персонажа</Form.Label>
          </Form.Group>
        </Col>
        <Col md={1} />
        <Col md={8}>
          <Row>
            <Col md={4}>
              <Form.Group controlId="characterAge" className="text-center">
                <Form.Control
                  type="text"
                  value={age}
                  onChange={handleAgeChange}
                  placeholder="Введите возраст"
                  aria-label="Возраст персонажа"
                />
                <Form.Label className="text-uppercase fw-bold">Возраст</Form.Label>
              </Form.Group>
            </Col>
            <Col md={4}>
              <Form.Group controlId="characterHeight" className="text-center">
                <Form.Control
                  type="text"
                  value={height}
                  onChange={handleHeightChange}
                  placeholder="Введите рост"
                  aria-label="Рост персонажа"
                />
                <Form.Label className="text-uppercase fw-bold">Рост</Form.Label>
              </Form.Group>
            </Col>
            <Col md={4}>
              <Form.Group controlId="characterWeight" className="text-center">
                <Form.Control
                  type="text"
                  value={weight}
                  onChange={handleWeightChange}
                  placeholder="Введите вес"
                  aria-label="Вес персонажа"
                />
                <Form.Label className="text-uppercase fw-bold">Вес</Form.Label>
              </Form.Group>
            </Col>
            <Col md={4}>
              <Form.Group controlId="characterEyes" className="text-center">
                <Form.Control
                  type="text"
                  value={eyes}
                  onChange={handleEyesChange}
                  placeholder="Введите цвет глаз"
                  aria-label="Цвет глаз персонажа"
                />
                <Form.Label className="text-uppercase fw-bold">Глаза</Form.Label>
              </Form.Group>
            </Col>
            <Col md={4}>
              <Form.Group controlId="characterSkin" className="text-center">
                <Form.Control
                  type="text"
                  value={skin}
                  onChange={handleSkinChange}
                  placeholder="Введите цвет кожи"
                  aria-label="Цвет кожи персонажа"
                />
                <Form.Label className="text-uppercase fw-bold">Кожа</Form.Label>
              </Form.Group>
            </Col>
            <Col md={4}>
              <Form.Group controlId="characterHair" className="text-center">
                <Form.Control
                  type="text"
                  value={hair}
                  onChange={handleHairChange}
                  placeholder="Введите цвет волос"
                  aria-label="Цвет волос персонажа"
                />
                <Form.Label className="text-uppercase fw-bold">Волосы</Form.Label>
              </Form.Group>
            </Col>
          </Row>
        </Col>
      </Row>
    </Container>
  );
};

export default Sheet2Header;