import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import CharacteristicCard from './CharacteristicCard';

function CharacteristicList({
  character,
  onCharacteristicChange
}) {
  //console.log(character)
  return (
    <Card className="border-0 d-flex flex-column justify-content-center align-items-center gap-3">
      <CharacteristicCard
        characteristic={{
          labelText: "Сила",
          baseControlId: "characterStrengthBase",
          bonusControlId: "characterStrengthBonus"
        }}
        character={character}
        attrName={"strength"}
        onCharacteristicChange={onCharacteristicChange}
      />
      <CharacteristicCard
        characteristic={{
          labelText: "Ловкость",
          baseControlId: "characterDexterityBase",
          bonusControlId: "characterDexterityBonus"
        }}
        character={character}
        attrName={"dexterity"}
        onCharacteristicChange={onCharacteristicChange}
      />
      <CharacteristicCard
        characteristic={{
          labelText: "Телосложение",
          baseControlId: "characterConstitutionBase",
          bonusControlId: "characterConstitutionBonus"
        }}
        character={character}
        attrName={"constitution"}
        onCharacteristicChange={onCharacteristicChange}
      />
      <CharacteristicCard
        characteristic={{
          labelText: "Интеллект",
          baseControlId: "characterIntelligenceBase",
          bonusControlId: "characterIntelligenceBonus"
        }}
        character={character}
        attrName={"intelligence"}
        onCharacteristicChange={onCharacteristicChange}
      />
      <CharacteristicCard
        characteristic={{
          labelText: "Мудрость",
          baseControlId: "characterWisdomBase",
          bonusControlId: "characterWisdomBonus"
        }}
        character={character}
        attrName={"wisdom"}
        onCharacteristicChange={onCharacteristicChange}
      />
      <CharacteristicCard
        characteristic={{
          labelText: "Харизма",
          baseControlId: "characterCharismaBase",
          bonusControlId: "characterCharismaBonus"
        }}
        character={character}
        attrName={"charisma"}
        onCharacteristicChange={onCharacteristicChange}
      />
    </Card>
  );
}

export default CharacteristicList;