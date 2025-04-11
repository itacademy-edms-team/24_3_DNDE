import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';

import InputHeader from './InputHeader';
import NameInputHeader from './NameInputHeader';

function Sheet1Header({
  character,
  characterName,
  onCharacterNameChange,
  onHeaderUpdate,
  onLevelUpdate,
  onExperienceUpdate
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
            <Col md={2}>
              <InputHeader
                controlId="characterClass"
                controlPlaceholder="Введите класс"
                controlLabel="Класс"
                character={character}
                attrName={"class"}
                onInputHeaderUpdate={onHeaderUpdate}
              />
            </Col>
            <Col md={2}>
              <InputHeader
                controlId="characterLevel"
                controlType="number"
                controlPlaceholder={1}
                controlLabel="Уровень"
                character={character}
                attrName={"level"}
                onInputHeaderUpdate={onLevelUpdate}
              />
            </Col>
            <Col md={4}>
              <InputHeader
                controlId="characterRace"
                controlPlaceholder="Введите расу"
                controlLabel="Раса"
                isReadOnly={true}
                character={character}
                attrName={"race"}
                onInputHeaderUpdate={onHeaderUpdate}
              />
            </Col>
            <Col md={4}>
              <InputHeader
                controlId="characterBackstory"
                controlPlaceholder="Введите предысторию"
                controlLabel="Предыстория"
                character={character}
                attrName={"backstory"}
                onInputHeaderUpdate={onHeaderUpdate}
              />
            </Col>
            <Col md={4}>
              <InputHeader
                controlId="characterWorldview"
                controlPlaceholder="Введите мировозрение"
                controlLabel="Мировоззрение"
                character={character}
                attrName={"worldview"}
                onInputHeaderUpdate={onHeaderUpdate}
              />
            </Col>
            <Col md={4}>
              <InputHeader
                controlId="characterPlayerName"
                controlPlaceholder="Введите ваше имя"
                controlLabel="Имя игрока"
                character={character}
                attrName={"playerName"}
                onInputHeaderUpdate={onHeaderUpdate}
              />
            </Col>
            <Col md={4}>
              <InputHeader
                controlId="characterExperience"
                controlType="number"
                controlPlaceholder="Введите опыт"
                controlLabel="Опыт"
                character={character}
                attrName={"experience"}
                onInputHeaderUpdate={onExperienceUpdate}
              />
            </Col>
          </Row>
        </Col>
      </Row>
    </Container>
  );
}

export default Sheet1Header;