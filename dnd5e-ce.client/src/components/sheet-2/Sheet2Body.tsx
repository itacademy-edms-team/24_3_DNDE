import Card from 'react-bootstrap/Card';
import Col from 'react-bootstrap/Col';
import Container from 'react-bootstrap/Container';
import Form from 'react-bootstrap/Form';
import Row from 'react-bootstrap/Row';

import CharacterBioAdditionalFeaturesAndTraitsSVG from './assets/CharacterBioAdditionalFeaturesAndTraits.svg';
import CharacterBioAlliesAndOrganizationsSVG from './assets/CharacterBioAlliesAndOrganizations.svg';
import CharacterBioAppearanceSVG from './assets/CharacterBioAppearance.svg';
import CharacterBioBackstorySVG from './assets/CharacterBioBackstory.svg';

import { useAppDispatch, useAppSelector } from '../../hooks';
import {
  selectAppearance,
  selectBackstory,
  selectAlliesAndOrganizations,
  selectAdditionalFeaturesAndTraits,
  selectTreasures,
} from '../../store/selectors/sheet2Selectors';
import {
    updateAdditionalFeaturesAndTraits,
    updateAlliesAndOrganizations,
    updateAppearance,
    updateBackstory,
    updateTreasures,
} from '../../store/sheet2Slice';

const Sheet2Body: React.FC = () => {
  const dispatch = useAppDispatch();
  const appearance = useAppSelector(selectAppearance);
  const backstory = useAppSelector(selectBackstory);
  const alliesAndOrganizations = useAppSelector(selectAlliesAndOrganizations);
  const additionalFeaturesAndTraits = useAppSelector(selectAdditionalFeaturesAndTraits);
  const treasures = useAppSelector(selectTreasures);

  const handleAppearanceChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    dispatch(updateAppearance(event.target.value));
  };

  const handleBackstoryChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    dispatch(updateBackstory(event.target.value));
  };

  const handleAlliesAndOrganizationsChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    dispatch(updateAlliesAndOrganizations(event.target.value));
  };

  const handleAdditionalFeaturesAndTraitsChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    dispatch(updateAdditionalFeaturesAndTraits(event.target.value));
  };

  const handleTreasuresChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    dispatch(updateTreasures(event.target.value));
  };

  return (
    <Container className="mt-4">
      <Row>
        <Col md={4} className="d-flex flex-column gap-3">
          <Card className="p-0 m-0 border-0">
            <Card.Img src={CharacterBioAppearanceSVG} />
            <Card.ImgOverlay className="p-0 m-0">
              <Form.Group controlId="characterAppearance" className="d-flex flex-column h-100">
                <Form.Control
                  as="textarea"
                  value={appearance}
                  className="flex-grow-1 bg-transparent border-0 shadow-none"
                  onChange={handleAppearanceChange}
                  aria-label="Внешний вид персонажа"
                />
                <div className="text-center">
                  <Form.Label className="text-uppercase fw-bold">
                    Внешний вид персонажа
                  </Form.Label>
                </div>
              </Form.Group>
            </Card.ImgOverlay>
          </Card>

          <Card className="p-0 m-0 border-0">
            <Card.Img src={CharacterBioBackstorySVG} />
            <Card.ImgOverlay className="p-0 m-0">
              <Form.Group controlId="characterBackstory" className="d-flex flex-column h-100">
                <Form.Control
                  as="textarea"
                  value={backstory}
                  className="flex-grow-1 bg-transparent border-0 shadow-none"
                  onChange={handleBackstoryChange}
                  aria-label="Предыстория персонажа"
                />
                <div className="text-center">
                  <Form.Label className="text-uppercase fw-bold">
                    Предыстория персонажа
                  </Form.Label>
                </div>
              </Form.Group>
            </Card.ImgOverlay>
          </Card>
        </Col>
        <Col md={8} className="d-flex flex-column gap-3">
          <Card className="p-0 m-0 border-0">
            <Card.Img src={CharacterBioAlliesAndOrganizationsSVG} />
            <Card.ImgOverlay className="p-0 m-0">
              <Form.Group controlId="characterAlliesAndOrganizations" className="d-flex flex-column h-100">
                <Form.Control
                  as="textarea"
                  value={alliesAndOrganizations}
                  className="flex-grow-1 bg-transparent border-0 shadow-none"
                  onChange={handleAlliesAndOrganizationsChange}
                  aria-label="Союзники и организации"
                />
                <div className="text-center">
                  <Form.Label className="text-uppercase fw-bold">
                    Союзники и Организации
                  </Form.Label>
                </div>
              </Form.Group>
            </Card.ImgOverlay>
          </Card>
          <Card className="p-0 m-0 border-0">
            <Card.Img src={CharacterBioAdditionalFeaturesAndTraitsSVG} />
            <Card.ImgOverlay className="p-0 m-0">
              <Form.Group controlId="characterAdditionalFeaturesAndTraits" className="d-flex flex-column h-100">
                <Form.Control
                  as="textarea"
                  value={additionalFeaturesAndTraits}
                  className="flex-grow-1 bg-transparent border-0 shadow-none"
                  onChange={handleAdditionalFeaturesAndTraitsChange}
                  aria-label="Дополнительные особенности и умения"
                />
                <div className="text-center">
                  <Form.Label className="text-uppercase fw-bold">
                    Дополнительные особенности и умения
                  </Form.Label>
                </div>
              </Form.Group>
            </Card.ImgOverlay>
          </Card>
          <Card className="p-0 m-0 border-0">
            <Card.Img src={CharacterBioAlliesAndOrganizationsSVG} />
            <Card.ImgOverlay className="p-0 m-0">
              <Form.Group controlId="characterTreasures" className="d-flex flex-column h-100">
                <Form.Control
                  as="textarea"
                  value={treasures}
                  placeholder="Ваши сокровища"
                  className="flex-grow-1 bg-transparent border-0 shadow-none"
                  onChange={handleTreasuresChange}
                  aria-label="Сокровища"
                />
                <div className="text-center">
                  <Form.Label className="text-uppercase fw-bold">
                    Сокровища
                  </Form.Label>
                </div>
              </Form.Group>
            </Card.ImgOverlay>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default Sheet2Body;