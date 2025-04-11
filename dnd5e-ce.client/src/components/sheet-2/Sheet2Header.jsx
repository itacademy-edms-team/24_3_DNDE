import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';

import InputHeader from './InputHeader';
import NameInputHeader from './NameInputHeader';

function Sheet2Header({
  character,
  characterName,
  onCharacterNameChange,
  onHeaderColChange
}) {
  return (
    <Container className="mt-3">
      <Row>
        <Col md={3} className="d-flex flex-column align-items-center justify-content-center">
          <NameInputHeader
            controlId="characterName"
            controlPlaceholder="Введите имя персонажа"
            controlLabel="Имя персонажа"
            characterName={characterName}
            onCharacterNameChange={onCharacterNameChange}
          />
        </Col>
        <Col md={1} />
        <Col md={8}>
          <Row>
            <Col md={4}>
              <InputHeader
                controlId="characterAge"
                controlPlaceholder="Введите возраст"
                controlLabel="Возраст"
                character={character}
                field={{ name: "age", sheet: "sheet2" }}
                onFieldChange={onHeaderColChange}
              />
            </Col>
            <Col md={4}>
              <InputHeader
                controlId="characterHeight"
                controlPlaceholder="Введите рост"
                controlLabel="Рост"
                character={character}
                field={{ name: "height", sheet: "sheet2" }}
                onFieldChange={onHeaderColChange}
              />
            </Col>
            <Col md={4}>
              <InputHeader
                controlId="characterWeight"
                controlPlaceholder="Введите вес"
                controlLabel="Вес"
                character={character}
                field={{ name: "weight", sheet: "sheet2" }}
                onFieldChange={onHeaderColChange}
              />
            </Col>
            <Col md={4}>
              <InputHeader
                controlId="characterEyes"
                controlPlaceholder="Введите цвет глаз"
                controlLabel="Глаза"
                character={character}
                field={{ name: "eyes", sheet: "sheet2" }}
                onFieldChange={onHeaderColChange}
              />
            </Col>
            <Col md={4}>
              <InputHeader
                controlId="characterSkin"
                controlPlaceholder="Введите цвет кожи"
                controlLabel="Кожа"
                character={character}
                field={{ name: "skin", sheet: "sheet2" }}
                onFieldChange={onHeaderColChange}
              />
            </Col>
            <Col md={4}>
              <InputHeader
                controlId="characterHair"
                controlPlaceholder="Введите цвет волос"
                controlLabel="Волосы"
                character={character}
                field={{ name: "hair", sheet: "sheet2" }}
                onFieldChange={onHeaderColChange}
              />
            </Col>
          </Row>
        </Col>
      </Row>
    </Container>
  );
}

export default Sheet2Header;