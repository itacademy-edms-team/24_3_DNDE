import Container from 'react-bootstrap/Container';
import Row from 'react-bootstrap/Row';
import Col from 'react-bootstrap/Col';
import Card from 'react-bootstrap/Card';
import Form from 'react-bootstrap/Form';

import CardBody from './CardBody';

import CharacterBioAlliesAndOrganizationsSVG from './assets/CharacterBioAlliesAndOrganizations.svg';
import CharacterBioAppearanceSVG from './assets/CharacterBioAppearance.svg';
import CharacterBioBackstorySVG from './assets/CharacterBioBackstory.svg';
import CharacterBioAdditionalFeaturesAndTraitsSVG from './assets/CharacterBioAdditionalFeaturesAndTraits.svg';


function Sheet2Body({
  character,
  onHeaderColChange
}) {
  return (
    <Container className='mt-4'>
      <Row>
        <Col md={4} className="d-flex flex-column gap-3">
          <CardBody
            imgObj={CharacterBioAppearanceSVG}
            controlId="characterBioAppearance"
            labelText="Внешний вид персонажа"
            character={character}
            field={ {name: "appearance", sheet: "sheet2"} }
            onHeaderColChange={onHeaderColChange}
          />
          <CardBody
            imgObj={CharacterBioBackstorySVG}
            controlId="characterBioBackstory"
            labelText="Предыстория персонажа"
            character={character}
            field={{ name: "backstory", sheet: "sheet2" }}
            onHeaderColChange={onHeaderColChange}
          />
        </Col>
        <Col md={8} className="d-flex flex-column gap-3">
          <CardBody
            imgObj={CharacterBioAlliesAndOrganizationsSVG}
            controlId="characterBioAlliesAndOrganizations"
            labelText="Союзники и организации"
            character={character}
            field={{ name: "alliesAndOrganizations", sheet: "sheet2" }}
            onHeaderColChange={onHeaderColChange}
          />
          <CardBody
            imgObj={CharacterBioAdditionalFeaturesAndTraitsSVG}
            controlId="characterBioAdditionalFeaturesAndTraits"
            labelText="Дополнительные особенности и умения"
            character={character}
            field={{ name: "additionalFeaturesAndTraits", sheet: "sheet2" }}
            onHeaderColChange={onHeaderColChange}
          />
          <CardBody
            imgObj={CharacterBioAdditionalFeaturesAndTraitsSVG}
            controlId="characterBioTreasures"
            labelText="Сокровища"
            character={character}
            field={{ name: "treasures", sheet: "sheet2" }}
            onHeaderColChange={onHeaderColChange}
          />
        </Col>
      </Row>
    </Container>
  );
}

export default Sheet2Body;